
using Microsoft.BizTalk.Component.Interop;
using Microsoft.BizTalk.Message.Interop;
using Microsoft.BizTalk.Streaming;
using System.IO;
using System.Runtime.InteropServices;
using System.Xml;
namespace Codit.LFT.PipelineComponents
{
    [ComponentCategory(CategoryTypes.CATID_PipelineComponent)]
    [ComponentCategory(CategoryTypes.CATID_Decoder)]
    [Guid("2587CB65-10CF-4631-9806-A90E2BDC21BD")]
    public class LFTDecoder : IBaseComponent, IComponent, IComponentUI, IPersistPropertyBag
    {
        #region IBaseComponent Members

        public string Description
        {
            get { return "Places incoming message in an archive location, and sends a reference to BizTalk"; }
        }

        public string Name
        {
            get { return "LFT Decoder"; }
        }

        public string Version
        {
            get { return "1.0.0.0"; }
        }

        #endregion

        #region IComponent Members

        public Microsoft.BizTalk.Message.Interop.IBaseMessage Execute(IPipelineContext pContext, Microsoft.BizTalk.Message.Interop.IBaseMessage pInMsg)
        {
            IBaseMessagePart bodyPart = pInMsg.BodyPart;
            Stream inboundStream = bodyPart.GetOriginalDataStream();
            VirtualStream virtualStream = new VirtualStream(0x280, 0x100000);
            ReadOnlySeekableStream readOnlySeekableStream = new ReadOnlySeekableStream(inboundStream, virtualStream, 0x280);
            string tempFile = Path.GetTempFileName();

            using (FileStream fs = new FileStream(tempFile, FileMode.Open, FileAccess.Write, FileShare.Read))
            {
                byte[] buffer = new byte[0x280];
                int bytesRead = readOnlySeekableStream.Read(buffer, 0, buffer.Length);
                while (bytesRead != 0)
                {
                    fs.Write(buffer, 0, bytesRead);
                    fs.Flush();
                    bytesRead = readOnlySeekableStream.Read(buffer, 0, buffer.Length);
                }
            }
            VirtualStream outputStream = new VirtualStream();

            using (XmlWriter xw = XmlWriter.Create(outputStream))
            {
                const string NameSpace = "http://Codit.LFT.Schemas";
                xw.WriteStartDocument();
                xw.WriteStartElement("ns0", "LFT", NameSpace);
                xw.WriteElementString("TempFile", tempFile);
                xw.WriteEndDocument();
            }

            outputStream.Position = 0;
            pContext.ResourceTracker.AddResource(outputStream);
            pInMsg.BodyPart.Data = outputStream;
            return pInMsg;
        }

        #endregion

        #region IComponentUI Members

        public System.IntPtr Icon
        {
            get { return new System.IntPtr(); }
        }

        public System.Collections.IEnumerator Validate(object projectSystem)
        {
            return null;
        }

        #endregion

        #region IPersistPropertyBag Members

        public void GetClassID(out System.Guid classID)
        {
            classID = new System.Guid("6B076AA7-4F32-4E70-9C48-8D0B682F7D56");
        }

        public void InitNew()
        {
        }

        public void Load(IPropertyBag propertyBag, int errorLog)
        {
        }

        public void Save(IPropertyBag propertyBag, bool clearDirty, bool saveAllProperties)
        {
        }

        #endregion
    }
}
