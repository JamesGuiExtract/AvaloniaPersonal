using System;
using System.Collections.Generic;
using Grpc.Net.Client;
using VMService;
using Extract.ErrorHandling;

namespace ExtractVMManager.Services
{
    public class Database
    {
        GrpcChannel channel = GrpcChannel.ForAddress("https://vmbackend.extract.local:5001");

        public IEnumerable<VirtualMachineModel> GetVirtualMachineModels()
        {
            VMManager.VMManagerClient client = new VMManager.VMManagerClient(channel);
            using var call = client.ListVMs(new NoParameterRequest());
            List<VirtualMachineModel> virtualMachineModels = new List<VirtualMachineModel>();
            try
            {
                while (call.ResponseStream.MoveNext(default).Result)
                {
                    var currentVM = call.ResponseStream.Current;
                    virtualMachineModels.Add(currentVM);
                }
            }
            catch (Exception ex)
            {
                ex.AsExtractException("ELI53667");
            }
            return virtualMachineModels;
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

        public void CreateNewVM(string? Name, string? TemplateName)
        {
            if(Name != null && TemplateName != null)
            {
                VMManager.VMManagerClient client = new VMManager.VMManagerClient(channel);
                client.CreateNewVirtualMachine(new CreateVirtualMachineRequest()
                {
                    TemplateName = TemplateName,
                    VirtualMachineName = Name,
                    CreatorName = Environment.UserName
                }) ;
            }
        }

        public IEnumerable<string>GetVMTemplates()
        {
            VMManager.VMManagerClient client = new VMManager.VMManagerClient(channel);
            var templates = client.GetTemplates(new NoParameterRequest());
            return templates.Template;
        }
    }
}
