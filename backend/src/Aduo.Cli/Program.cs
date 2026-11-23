using Aduo.Cli;

// Aduo.Cli – công cụ dòng lệnh dùng chung domain với API (docs/phase-3/04-folder-structure.md §2.4).
// Các lệnh khác (seed, import wiktionary, import tatoeba, tts generate) được thêm dần theo từng module.

if (args.Length == 0 || args[0] is "-h" or "--help")
{
    Console.WriteLine("""
        aduo-cli – công cụ quản trị ADUO

        Lệnh hiện có:
          create-admin --email <email> [--name <tên hiển thị>]
              Tạo (hoặc nâng quyền) tài khoản Admin. Mật khẩu nhập ẩn khi chạy,
              hoặc đặt biến môi trường ADUO_ADMIN_PASSWORD (không truyền mật khẩu qua tham số dòng lệnh).

        Kết nối DB: biến môi trường ConnectionStrings__Aduo (mặc định môi trường Development dùng DB local).

        Các lệnh sẽ có sau:
          seed               Nạp dữ liệu tra cứu                [ContentOps / Catalog]
          import wiktionary  Nhập từ vựng từ dump kaikki.org     [Lexicon]
          import tatoeba     Nhập câu ví dụ từ Tatoeba           [Lexicon]
          tts generate       Tạo audio TTS                       [ContentOps]
        """);
    return 0;
}

return args[0] switch
{
    "create-admin" => await CreateAdminCommand.RunAsync(args[1..]),
    _ => Fail($"Lệnh không tồn tại: {args[0]}. Chạy aduo-cli --help."),
};

static int Fail(string message)
{
    Console.Error.WriteLine(message);
    return 1;
}
