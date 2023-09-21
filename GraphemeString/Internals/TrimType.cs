namespace TsukuyoOka.Text.Unicode.Internals;

[Flags]
internal enum TrimType
{
    Start = 0x01,
    End = 0x02,
    Both = Start | End
}