using System;
using System.Collections.Generic;
using Grpc.Net.Client;
using VMService;

namespace ExtractVMManager.Services
{
    public class Database
    {
        GrpcChannel channel = GrpcChannel.ForAddress("https://vmbackend.extract.local:5001");

        public IEnumerable<VirtualMachineModel> GetItems()
        {
            VMManager.VMManagerClient client = new VMManager.VMManagerClient(channel);
            using var call = client.ListVMs(new NoParameterRequest());
            List<VirtualMachineModel> test = new List<VirtualMachineModel>();
            try
            {
                while (call.ResponseStream.MoveNext(default).Result)
                {
                    var currentVM = call.ResponseStream.Current;
                    test.Add(currentVM);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            return test;
        }

        public void StartVM(string VMName)
        {
            VMManager.VMManagerClient client = new VMManager.VMManagerClient(channel);
            client.Start(new VMRequest() { VirtualMachineName = VMName });
        }

        public void StopVM(string VMName)
        {
            VMManager.VMManagerClient client = new VMManager.VMManagerClient(channel);
            client.Stop(new VMRequest() { VirtualMachineName = VMName });
        }

        public void RestartVM(string VMName)
        {
            VMManager.VMManagerClient client = new VMManager.VMManagerClient(channel);
            client.Reset(new VMRequest() { VirtualMachineName = VMName });
        }

        public void CreateNewVM(string? Name, int? TemplateIndex)
        {
            if(Name != null && TemplateIndex != null)
            {
                string Template = getTemplateFromIndex(TemplateIndex);
                VMManager.VMManagerClient client = new VMManager.VMManagerClient(channel);
                client.CreateNewVirtualMachine(new CreateVirtualMachineRequest()
                {
                    TemplateName = Template,
                    VirtualMachineName = Name
                });
            }
        }

        private string getTemplateFromIndex(int? TemplateIndex)
        {
            switch (TemplateIndex)
            {
                case 0:
                    return "DevSQL2019";
                default:
                    return "";
            }
        }
    }
}
