using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Threading.Tasks;
using Akavache;
using SimpleQ.Webinterface.Models;
using Xamarin.Forms;
using System.Reactive.Linq;
using System.Diagnostics;
using System.Linq;
using SimpleQ.Models;

namespace SimpleQ.PageModels.Services
{
    public class FaqService : IFaqService
    {
        private ObservableCollection<FAQModel> faqEntries;
        private IWebAPIService webAPIService;

        public FaqService(IWebAPIService webAPIService) : this()
        {
            this.webAPIService = webAPIService;
        }

        public FaqService()
        {
            this.faqEntries = new ObservableCollection<FAQModel>();
        }

        public ObservableCollection<FAQModel> FaqEntries
        {
            get => faqEntries;
        }

        public void AddFaqEntry(FAQModel entry)
        {
            if (entry.Entry.IsMobile)
            {
                this.faqEntries.Add(entry);
            }
        }

        public void AddFaqEntries(List<FAQModel> faqEntries)
        {
            foreach (FAQModel entry in faqEntries)
            {
                AddFaqEntry(entry);
            }
        }

        public void ResetFaqEntries()
        {
            this.faqEntries.Clear();
        }

        public async Task LoadData()
        {
            Device.BeginInvokeOnMainThread(async () =>
            {
                try
                {

                    BlobCache.LocalMachine.GetAndFetchLatest<List<FaqEntry>>("FaqEntries", async () => await webAPIService.GetFaqEntries(), null, null).Subscribe(entries => {
                        if (entries != null)
                        {
                            ResetFaqEntries();
                            foreach (FaqEntry entry in entries)
                            {
                                AddFaqEntry(new FAQModel(entry));
                            }
                        }

                    });
                }
                catch
                {

                }

            });
        }
    }
}
