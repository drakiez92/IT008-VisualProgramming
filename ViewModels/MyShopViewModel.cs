﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using GoninDigital.Models;
using GoninDigital.Properties;
using Microsoft.EntityFrameworkCore;
using GoninDigital.SharedControl;
using ModernWpf.Controls;
using System.Windows.Input;

namespace GoninDigital.ViewModels
{
    class MyShopViewModel : BaseViewModel
    {
        private Product selectedItem=null;
        public Product SelectedItem
        {
            get { return selectedItem; }
            set { selectedItem = value; OnPropertyChanged(); }
        }
        private Vendor vendor = null;

        public Vendor Vendor
        {
            get { return vendor; }
            set { vendor = value; OnPropertyChanged(); }
        }

        private ObservableCollection<Product> products = null;
        public ObservableCollection<Product> Products
        {
            get { return products; }
            set { products = value; OnPropertyChanged(); }
        }

        public int OnPrimaryButtonClick { get; private set; }
        public int PrimaryButtonClick { get; private set; }

        private void InitVendor()
        {
            using (var db = new GoninDigitalDBContext())
            {
                try
                {
                    Vendor = db.Vendors.Include(o => o.Owner)
                        .Include(o => o.Products)
                        .Single(o => o.Owner.UserName == Settings.Default.usrname);
                    /*Products = db.Products.Where(o => o.VendorId == Vendor.Id).ToList();*/
                    Products = new ObservableCollection<Product>(Vendor.Products.ToList());
                }
                catch
                {

                    //MessageBox.Show("Cannot find out any vendors ");
                }
            }
        }
        public void OnNavigatedTo()
        {

        }
        public ICommand EditCommand { get; set; }
        public void EditCommandExec(Product product)
        {
            SelectedItem = product;
            var dialog = new ContentDialog
            {
                Content = new EditProductDialog(),

                Title = "Title",
                PrimaryButtonText = "Yes",
                SecondaryButtonText = "No",
                CloseButtonText = "Cancel",
                DefaultButton = ContentDialogButton.Primary,
                PrimaryButtonCommand = new RelayCommand<object>((p)=>true,p=>MessageBox.Show("sdsad")),
                /*PrimaryButtonClick += OnPrimaryButtonClick,
                SecondaryButtonClick += OnSecondaryButtonClick,
                CloseButtonClick += OnCloseButtonClick,
                Closed += OnClosed*/
            };
            dialog.ShowAsync();
        }
        public void RemoveProduct(Product product)
        {
            using (var db = new GoninDigitalDBContext())
            {
                try
                {
                    db.Products.Remove(product);
                    db.SaveChanges();
                }
                catch
                {

                    //MessageBox.Show("Cannot find out any vendors ");
                }
            }
        }
        public MyShopViewModel()
        {
            EditCommand = new RelayCommand<Product>(o => true, o => EditCommandExec(o));
            Thread thread = new Thread(InitVendor);
            thread.Start();

        }
    }
}
