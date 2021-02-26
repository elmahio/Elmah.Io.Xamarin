using ElmahIo.Samples.XamarinForms.ViewModels;
using System.ComponentModel;
using Xamarin.Forms;

namespace ElmahIo.Samples.XamarinForms.Views
{
    public partial class ItemDetailPage : ContentPage
    {
        public ItemDetailPage()
        {
            InitializeComponent();
            BindingContext = new ItemDetailViewModel();
        }
    }
}