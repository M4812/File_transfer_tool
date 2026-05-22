using System.Xml.Linq;

namespace FileTransferTool.App.Models;

internal sealed class SendLinkConfig
{
    // 发送链路描述“监控哪个目录、往哪里发、发完后如何归档”。
    public string LinkName { get; set; } = string.Empty;
    public string MonitorPath { get; set; } = string.Empty;
    public string RemoteAddress { get; set; } = string.Empty;
    public int RemotePort { get; set; }
    public string BackupPath { get; set; } = string.Empty;
    public string LinkIdentity { get; set; } = string.Empty;
    public string FilterType { get; set; } = string.Empty;

    public XElement ToXml()
    {
        return new XElement(
            "链路信息",
            new XElement("链路名称", LinkName),
            new XElement("监视目录", MonitorPath),
            new XElement("发送地址", RemoteAddress),
            new XElement("发送端口", RemotePort),
            new XElement("备份目录", BackupPath),
            new XElement("链路标识", LinkIdentity),
            new XElement("过滤类型", FilterType));
    }
}

internal sealed class ReceiveLinkConfig
{
    // 接收链路描述“监听到某个链路标识后，应该落到哪个目录”。
    public string LinkName { get; set; } = string.Empty;
    public string RemoteIP { get; set; } = string.Empty;
    public string ReceivePath { get; set; } = string.Empty;
    public string LinkIdentity { get; set; } = string.Empty;

    public XElement ToXml()
    {
        return new XElement(
            "链路信息",
            new XElement("链路名称", LinkName),
            new XElement("远端IP", RemoteIP),
            new XElement("接收目录", ReceivePath),
            new XElement("链路标识", LinkIdentity));
    }
}

internal sealed class LinkConfigDocument
{
    // 整个配置文件同时包含本机的发送链路和接收链路。
    public List<SendLinkConfig> SendLinks { get; } = [];
    public List<ReceiveLinkConfig> ReceiveLinks { get; } = [];
    public string ReceiveIP { get; set; } = "0.0.0.0";
    public int ReceivePort { get; set; } = 12092;
}
