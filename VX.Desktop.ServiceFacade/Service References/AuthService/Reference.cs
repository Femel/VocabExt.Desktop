﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.261
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace VX.Desktop.ServiceFacade.AuthService {
    
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    [System.ServiceModel.ServiceContractAttribute(ConfigurationName="AuthService.IAuthService")]
    public interface IAuthService {
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IAuthService/Login", ReplyAction="http://tempuri.org/IAuthService/LoginResponse")]
        System.Guid Login(string userName, string password);
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public interface IAuthServiceChannel : VX.Desktop.ServiceFacade.AuthService.IAuthService, System.ServiceModel.IClientChannel {
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public partial class AuthServiceClient : System.ServiceModel.ClientBase<VX.Desktop.ServiceFacade.AuthService.IAuthService>, VX.Desktop.ServiceFacade.AuthService.IAuthService {
        
        public AuthServiceClient() {
        }
        
        public AuthServiceClient(string endpointConfigurationName) : 
                base(endpointConfigurationName) {
        }
        
        public AuthServiceClient(string endpointConfigurationName, string remoteAddress) : 
                base(endpointConfigurationName, remoteAddress) {
        }
        
        public AuthServiceClient(string endpointConfigurationName, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(endpointConfigurationName, remoteAddress) {
        }
        
        public AuthServiceClient(System.ServiceModel.Channels.Binding binding, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(binding, remoteAddress) {
        }
        
        public System.Guid Login(string userName, string password) {
            return base.Channel.Login(userName, password);
        }
    }
}
