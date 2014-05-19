using Microsoft.BizTalk.Component.Interop;
using Microsoft.BizTalk.Message.Interop;
using Microsoft.BizTalk.Streaming;
using Microsoft.BizTalk.XPath;
using System.IO;
using System.Runtime.InteropServices;
using System.Xml;
namespace Codit.LFT.PipelineComponents
{
    [ComponentCategory(CategoryTypes.CATID_PipelineComponent)]
    [ComponentCategory(CategoryTypes.CATID_Encoder)]
    [Guid("35F574A0-DF1B-4C55-8B9D-E7992B0AB4A4")]
    public class LFTEncoder : IBaseComponent, IComponent, IComponentUI, IPersistPropertyBag
    {
        #region IBaseComponent Members

        public string Description
        {
            get { return "Takes message from temporary location and transfers to destination"; }
        }

        public string Name
        {
            get { return "LFT Encoder"; }
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
            XmlTextReader xmlTextReader = new XmlTextReader(readOnlySeekableStream);
            XPathCollection xPathCollection = new XPathCollection();
            xPathCollection.Add("/*[local-name()='LFT' and namespace-uri()='http://Codit.LFT.Schemas']/*[local-name()='TempFile' and namespace-uri()='']");
            XPathReader xPathReader = new XPathReader(xmlTextReader, xPathCollection);
            bool ok = false;
            string val = string.Empty;
            while (xPathReader.ReadUntilMatch())
            {
                if (xPathReader.Match(0) && !ok)
                {
                    val = xPathReader.ReadString();
                    ok = true;
                }
            }
            if (ok)
            {
                VirtualStream outboundStream = new VirtualStream(0x280, 0xA00000);
                using (FileStream fs = new FileStream(val, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    byte[] buffer = new byte[4096];
                    int bytesRead = fs.Read(buffer, 0, buffer.Length);
                    while (bytesRead != 0)
                    {
                        outboundStream.Write(buffer, 0, bytesRead);
                        outboundStream.Flush();
                        bytesRead = fs.Read(buffer, 0, buffer.Length);
                    }
                }
                outboundStream.Position = 0;
                bodyPart.Data = outboundStream;
            }

            return pInMsg;
        }

        #endregion

        #region IComponentUI Members

        public System.IntPtr Icon
        {
            get
            {
                return new System.IntPtr();
            }
        }

        public System.Collections.IEnumerator Validate(object projectSystem)
        {
            return null;
        }

        #endregion

        #region IPersistPropertyBag Members

        public void GetClassID(out System.Guid classID)
        {
            classID = new System.Guid("3CAE6FA5-A55C-4609-8891-651B577FDA19");
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
