﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:2.0.50727.1318
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace SnitzProvider.Properties {
    
    
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "9.0.0.0")]
    internal sealed partial class Settings : global::System.Configuration.ApplicationSettingsBase {
        
        private static Settings defaultInstance = ((Settings)(global::System.Configuration.ApplicationSettingsBase.Synchronized(new Settings())));
        
        public static Settings Default {
            get {
                return defaultInstance;
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.SpecialSettingAttribute(global::System.Configuration.SpecialSetting.ConnectionString)]
        [global::System.Configuration.DefaultSettingValueAttribute("Data Source=.\\SQLEXPRESS;AttachDbFilename=\"C:\\Documents and Settings\\JamesCurran\\" +
            "My Documents\\Visual Studio\\Projects\\SnitzMemberRole\\Database\\NJT_Memebers.mdf\";I" +
            "ntegrated Security=True;Connect Timeout=30;User Instance=True")]
        public string NJT_MemebersConnectionString {
            get {
                return ((string)(this["NJT_MemebersConnectionString"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.SpecialSettingAttribute(global::System.Configuration.SpecialSetting.ConnectionString)]
        [global::System.Configuration.DefaultSettingValueAttribute("Data Source=NANAK\\SQLEXPRESS;Initial Catalog=\"C:\\DOCUMENTS AND SETTINGS\\JAMESCURR" +
            "AN\\MY DOCUMENTS\\VISUAL STUDIO\\PROJECTS\\SNITZMEMBERROLE\\DATABASE\\NJT_MEMEBERS.MDF" +
            "\";Integrated Security=True")]
        public string C__DOCUMENTS_AND_SETTINGS_JAMESCURRAN_MY_DOCUMENTS_VISUAL_STUDIO_PROJECTS_SNITZMEMBERROLE_DATABASE_NJT_MEMEBERS_MDFConne {
            get {
                return ((string)(this["C__DOCUMENTS_AND_SETTINGS_JAMESCURRAN_MY_DOCUMENTS_VISUAL_STUDIO_PROJECTS_SNITZME" +
                    "MBERROLE_DATABASE_NJT_MEMEBERS_MDFConne"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.SpecialSettingAttribute(global::System.Configuration.SpecialSetting.ConnectionString)]
        [global::System.Configuration.DefaultSettingValueAttribute("Data Source=NANAK\\SQLEXPRESS;Initial Catalog=SnitzProviderTest;Integrated Securit" +
            "y=True")]
        public string SnitzProviderTestConnectionString {
            get {
                return ((string)(this["SnitzProviderTestConnectionString"]));
            }
        }
    }
}
