// Aduo.Cli – công cụ dòng lệnh dùng chung domain với API (docs/phase-3/04-folder-structure.md §2.4).
// Các lệnh (seed, create-admin, import wiktionary, import tatoeba, tts generate) được thêm dần theo từng module.

Console.WriteLine("""
    aduo-cli – công cụ quản trị ADUO

    Các lệnh sẽ có:
      seed               Nạp dữ liệu tra cứu (licenses, sources, levels…)   [ContentOps / Catalog]
      create-admin       Tạo tài khoản Admin đầu tiên                         [Identity]
      import wiktionary  Nhập từ vựng từ dump kaikki.org                       [Lexicon]
      import tatoeba     Nhập câu ví dụ từ Tatoeba                             [Lexicon]
      tts generate       Tạo audio TTS cho nội dung chưa có audio              [ContentOps]
    """);

return args.Length == 0 ? 0 : 1;
