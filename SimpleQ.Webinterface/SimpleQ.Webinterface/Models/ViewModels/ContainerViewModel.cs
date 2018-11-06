using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SimpleQ.Webinterface.Models.ViewModels
{
    public class ContainerViewModel
    {
        public SurveyCreationModel SurveyCreationModel { get; set; }
        public SurveyResultsModel SurveyResultsModel { get; set; }
        public GroupAdministrationModel GroupAdministrationModel { get; set; }
        public SettingsModel SettingsModel { get; set; }
    }
}