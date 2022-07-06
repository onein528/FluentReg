using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.AppService;
using Windows.Foundation.Collections;

namespace FluentReg.Bridge.MessageHandlers
{
    public interface IMessageHandler
    {
        Task ParseArgumentsAsync(AppServiceConnection sender, AppServiceRequestReceivedEventArgs args, string arguments, Dictionary<string, object> message);

        void Initialize(AppServiceConnection connection);
    }
}
