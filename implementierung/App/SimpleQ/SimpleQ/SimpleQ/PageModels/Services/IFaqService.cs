using SimpleQ.Models;
using SimpleQ.Webinterface.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Threading.Tasks;

namespace SimpleQ.PageModels.Services
{
    public interface IFaqService
    {
        ObservableCollection<FAQModel> FaqEntries { get; }

        void AddFaqEntry(FAQModel entry);
        void AddFaqEntries(List<FAQModel> entries);
        Task LoadData();

        void ResetFaqEntries();

    }
}
