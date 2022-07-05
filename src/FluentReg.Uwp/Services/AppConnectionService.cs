using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.AppService;
using Windows.ApplicationModel.Background;
using Windows.Foundation.Collections;

namespace FluentReg.Uwp.Services
{
    public class AppConnectionService
    {
        public AppServiceConnection Connection { get; set; }
        private BackgroundTaskDeferral AppServiceDeferral { get; set; }

        public async Task InitializeConnection(BackgroundActivatedEventArgs args)
        {
            if (args.TaskInstance.TriggerDetails is AppServiceTriggerDetails appService)
            {
                AppServiceDeferral = args.TaskInstance.GetDeferral();
                args.TaskInstance.Canceled += TaskInstance_Canceled;
                Connection = appService.AppServiceConnection;
                Connection.RequestReceived += AppServiceConnection_RequestReceived;
                Connection.ServiceClosed += AppServiceConnection_ServiceClosed;

                await Views.MainPage.Current.ViewModel.ConnectionHandler();
            }
        }

        private void AppServiceConnection_ServiceClosed(AppServiceConnection sender, AppServiceClosedEventArgs args)
        {
            AppServiceDeferral?.Complete();
        }

        private void AppServiceConnection_RequestReceived(AppServiceConnection sender, AppServiceRequestReceivedEventArgs args)
        {
            var d = args.GetDeferral();

            // TODO

            d.Complete();
        }

        private void TaskInstance_Canceled(IBackgroundTaskInstance sender, BackgroundTaskCancellationReason reason)
        {
            AppServiceDeferral?.Complete();
        }
    }
}
