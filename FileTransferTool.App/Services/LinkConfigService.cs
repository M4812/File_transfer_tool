using System.Xml.Linq;
using FileTransferTool.App.Models;

namespace FileTransferTool.App.Services;

internal static class LinkConfigService
{
    // 把旧版中文 XML 配置文件读取成运行时模型，便于界面和收发服务直接使用。
    public static LinkConfigDocument Load(string path)
    {
        var document = new LinkConfigDocument();
        if (!File.Exists(path))
        {
            return document;
        }

        // 兼容原工具的中文 XML 结构，便于直接复用已有链路配置。
        var root = XElement.Load(path);
        var sendRoot = root.Element("发送链路");
        if (sendRoot is not null)
        {
            foreach (var item in sendRoot.Elements("链路信息"))
            {
                document.SendLinks.Add(new SendLinkConfig
                {
                    LinkName = Value(item, "链路名称"),
                    MonitorPath = Value(item, "监视目录"),
                    RemoteAddress = Value(item, "发送地址"),
                    RemotePort = IntValue(item, "发送端口", 12092),
                    BackupPath = Value(item, "备份目录"),
                    LinkIdentity = Value(item, "链路标识"),
                    FilterType = Value(item, "过滤类型")
                });
            }
        }

        var receiveRoot = root.Element("接收链路");
        if (receiveRoot is not null)
        {
            document.ReceiveIP = Value(receiveRoot, "接收IP", "0.0.0.0");
            document.ReceivePort = IntValue(receiveRoot, "接收端口", 12092);
            foreach (var item in receiveRoot.Elements("链路信息"))
            {
                document.ReceiveLinks.Add(new ReceiveLinkConfig
                {
                    LinkName = Value(item, "链路名称"),
                    RemoteIP = Value(item, "远端IP"),
                    ReceivePath = Value(item, "接收目录"),
                    LinkIdentity = Value(item, "链路标识")
                });
            }
        }

        return document;
    }

    public static void Save(string path, LinkConfigDocument document)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        // 输出结构尽量贴近旧版发布包中的 linkdata.xml。
        var root = new XElement(
            "链路配置",
            new XElement("发送链路", document.SendLinks.Select(x => x.ToXml())),
            new XElement(
                "接收链路",
                new XElement("接收端口", document.ReceivePort),
                new XElement("接收IP", document.ReceiveIP),
                document.ReceiveLinks.Select(x => x.ToXml())));

        new XDocument(new XDeclaration("1.0", "utf-8", "yes"), root).Save(path);
    }

    private static string Value(XElement element, string name, string defaultValue = "")
    {
        return element.Element(name)?.Value.Trim() ?? defaultValue;
    }

    private static int IntValue(XElement element, string name, int defaultValue)
    {
        return int.TryParse(Value(element, name), out var value) ? value : defaultValue;
    }
}
