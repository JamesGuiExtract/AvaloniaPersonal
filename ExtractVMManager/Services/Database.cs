using System;
using System.Collections.Generic;
using Grpc.Net.Client;
using VMService;
using Extract.ErrorHandling;
using System.Linq;

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

        public void CreateNewVM(string? Name, string? TemplateName, string? purpose)
        {
            if(Name != null && TemplateName != null)
            {
                VMManager.VMManagerClient client = new VMManager.VMManagerClient(channel);
                client.CreateNewVirtualMachine(new CreateVirtualMachineRequest()
                {
                    TemplateName = TemplateName,
                    VirtualMachineName = Name,
                    CreatorName = Environment.UserName,
                    Purpose = purpose
                }) ;
            }
        }

        public IEnumerable<string>GetVMTemplates()
        {
            VMManager.VMManagerClient client = new VMManager.VMManagerClient(channel);
            using var call = client.GetTemplates(new NoParameterRequest());
            List<TemplateModel> templates = new List<TemplateModel>();
            try
            {
                while(call.ResponseStream.MoveNext(default).Result)
                {
                    var currentTemplate = call.ResponseStream.Current;
                    templates.Add(currentTemplate);
                }
            }
            catch(Exception ex)
            {
                ex.AsExtractException("ELI53976");
            }
            return templates.Select(t => t.TemplateName);
        }

        public void  JoinDomain(string VMName)
        {
            VMManager.VMManagerClient client = new VMManager.VMManagerClient(channel);
            var request = new DomainJoinRequest() { VirtualMachineName=VMName, DomainToJoin="extract.local" };
            client.DomainJoin(request);
        }
    }
}
