using System.Reflection;
using System.Reflection.Emit;
using Aduo.Core.Common.Content;

namespace Aduo.ArchitectureTests;

/// <summary>
/// DoD của ContentOps: "không có đường nào publish mà không đi qua PublishGuard". Cách đảm bảo: chỉ
/// <see cref="ContentLifecycle"/> được gọi setter <c>ContentMeta.Status</c>. Test quét IL của mọi method trong Aduo.Core.
/// </summary>
public class ContentLifecycleBoundaryTests
{
    private static readonly Assembly Core = typeof(ContentMeta).Assembly;

    private static readonly Dictionary<short, OpCode> OpCodes = typeof(OpCodes)
        .GetFields(BindingFlags.Public | BindingFlags.Static)
        .Select(f => (OpCode)f.GetValue(null)!)
        .ToDictionary(o => o.Value);

    [Fact]
    public void OnlyContentLifecycle_ChangesStatus()
    {
        var statusSetter = typeof(ContentMeta).GetProperty(nameof(ContentMeta.Status))!.GetSetMethod(nonPublic: true)!;
        var offenders = new List<string>();

        foreach (var type in Core.GetTypes())
        {
            var allowed = type == typeof(ContentLifecycle) || type == typeof(ContentMeta);
            if (allowed)
            {
                continue;
            }

            const BindingFlags all = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;
            foreach (var method in type.GetMethods(all).Cast<MethodBase>().Concat(type.GetConstructors(all)))
            {
                if (CallsMethod(method, statusSetter))
                {
                    offenders.Add($"{type.FullName}.{method.Name}");
                }
            }
        }

        Assert.True(offenders.Count == 0, "Chỉ ContentLifecycle được đổi ContentMeta.Status. Vi phạm: " + string.Join(", ", offenders));
    }

    [Fact]
    public void Scanner_DetectsAKnownCaller() // bảo đảm bộ quét không "mù": ContentLifecycle thật sự gọi setter
    {
        var setter = typeof(ContentMeta).GetProperty(nameof(ContentMeta.Status))!.GetSetMethod(nonPublic: true)!;
        var publish = typeof(ContentLifecycle).GetMethod(nameof(ContentLifecycle.Publish))!;

        Assert.True(CallsMethod(publish, setter));
    }

    private static bool CallsMethod(MethodBase method, MethodInfo target)
    {
        var body = method.GetMethodBody();
        var il = body?.GetILAsByteArray();
        if (il is null)
        {
            return false;
        }

        var module = method.Module;
        var i = 0;
        while (i < il.Length)
        {
            var op = il[i] == 0xFE ? OpCodes[(short)(0xFE00 | il[i + 1])] : OpCodes[il[i]];
            i += op.Size;

            if (op.OperandType == OperandType.InlineMethod)
            {
                var token = BitConverter.ToInt32(il, i);
                try
                {
                    var resolved = module.ResolveMethod(token, method.DeclaringType?.GetGenericArguments(), method.IsGenericMethod ? method.GetGenericArguments() : null);
                    if (resolved is not null && resolved.MetadataToken == target.MetadataToken && resolved.Module == target.Module)
                    {
                        return true;
                    }
                }
                catch (ArgumentException)
                {
                    // token không phải method của module này, bỏ qua
                }
            }

            i += OperandSize(op, il, i);
        }

        return false;
    }

    private static int OperandSize(OpCode op, byte[] il, int position) => op.OperandType switch
    {
        OperandType.InlineNone => 0,
        OperandType.ShortInlineBrTarget or OperandType.ShortInlineI or OperandType.ShortInlineVar => 1,
        OperandType.InlineVar => 2,
        OperandType.InlineI8 or OperandType.InlineR => 8,
        OperandType.InlineSwitch => 4 + (BitConverter.ToInt32(il, position) * 4),
        _ => 4,
    };
}
