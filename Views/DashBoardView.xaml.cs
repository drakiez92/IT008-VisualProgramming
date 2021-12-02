﻿using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using GoninDigital.Models;
using GoninDigital.Views.DashBoardPages;
using ModernWpf.Controls;
using ModernWpf.Controls.Primitives;
using System.Linq;
using Frame = System.Windows.Controls.Frame;
using Page = ModernWpf.Controls.Page;
using GoninDigital.Views.SharedPages;
using GoninDigital.Properties;
using System.Windows.Media.Imaging;
using ListViewItem = ModernWpf.Controls.ListViewItem;
using GoninDigital.Utils;

namespace GoninDigital.Views
{
    class SearchItem
    {
        public enum ItemType
        {
            VENDOR, PRODUCT
        }
        public int Id { get; set; }

        public string Name { get; set; }
        public string Description { get; set; }
        public string Image { get; set; }
        public ItemType Type { get; set; }
        public override string ToString()
        {
            return Name;
        }
    }

    
    public partial class DashBoard : UserControl
    {
        private static Frame rootFrame;
        public static Frame RootFrame
        {
            get => rootFrame;
        }

        Dictionary<string, Page> pages;
        public User currentUser = null;

        // Flyout currently not support binding data
        // Use behind code to generate UI instead
        public StackPanel userFlyoutContent = null;
        
        public DashBoard()
        {
            InitializeComponent();
            DataContext = this;
            rootFrame = contentFrame;
            pages = new Dictionary<string, Page>();
        }

        private void NavigationView_SelectionChanged(ModernWpf.Controls.NavigationView sender, ModernWpf.Controls.NavigationViewSelectionChangedEventArgs args)
        {
            if (args.IsSettingsSelected)
            {
                contentFrame.Navigate(typeof(SettingPage));
                return;
            }
            var selectedItem = (ModernWpf.Controls.NavigationViewItem)args.SelectedItem;
            if (selectedItem != null)
            {
                string selectedItemTag = (string)selectedItem.Tag;
                string pageName = "GoninDigital.Views.DashBoardPages." + selectedItemTag;
                Page togo;
                if (!pages.TryGetValue(pageName, out togo))
                {
                    Type pageType = typeof(HomePage).Assembly.GetType(pageName);
                    togo = (Page)Activator.CreateInstance(pageType);
                    pages.Add(pageName, togo);
                }
                contentFrame.Navigate(togo);
            }
            else
            {
                contentFrame.Navigate(typeof(HomePage));
            }
        }

        private void navigationView_BackRequested(NavigationView sender, NavigationViewBackRequestedEventArgs args)
        {
            contentFrame.GoBack();
        }

        private void ContentFrame_Navigated(object sender, NavigationEventArgs e)
        {
            if (contentFrame.CanGoBack)
                navigationView.IsBackEnabled = true;
            else
                navigationView.IsBackEnabled = false;

            var desType = contentFrame.SourcePageType;
            if (desType == typeof(HomePage))
            {
                homeItem.IsSelected = true;
            }
            else if (desType == typeof(CartPage))
            {
                cartItem.IsSelected = true;
            }
            else if (desType == typeof(SettingPage))
            {
                ((NavigationViewItem)navigationView.SettingsItem).IsSelected = true;
            }
            else if (desType == typeof(OrderPage))
            {
                orderItem.IsSelected = true;
            }
            else if (desType == typeof(MyShopPage))
            {
                myShopItem.IsSelected = true;
            }
            
        }

        private void NavigationViewItem_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (currentUser == null)
            {
                using (var db = new GoninDigitalDBContext())
                {
                    currentUser = db.Users.FirstOrDefault(o => o.UserName == Settings.Default.usrname);
                }
                userFlyoutContent = new StackPanel()
                {
                    Orientation = Orientation.Vertical,
                };
                var avatar = new PersonPicture()
                {
                    DisplayName = currentUser.FirstName + " " + currentUser.LastName,
                    ProfilePicture = currentUser.Avatar != null ? new BitmapImage(new Uri(currentUser.Avatar, UriKind.Absolute)) : null,
                    Margin = new Thickness(20, 10, 20, 5),
                };
                var name = new Label()
                {
                    Content = currentUser.FirstName + " " + currentUser.LastName,
                    FontWeight = FontWeights.Bold,
                    FontSize = 18,
                    HorizontalAlignment = HorizontalAlignment.Center,
                };
                var username = new Label()
                {
                    Content = "@" + currentUser.UserName,
                    HorizontalAlignment = HorizontalAlignment.Center,
                };
                userFlyoutContent.Children.Add(avatar);
                userFlyoutContent.Children.Add(name);
                userFlyoutContent.Children.Add(username);
                userFlyoutContent.Children.Add((UIElement)Resources["ok"]);
                flyout.Content = userFlyoutContent;
            }
            
            FlyoutBase.ShowAttachedFlyout((FrameworkElement)sender);
        }

        private void AutoSuggestBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
            {
                var content = sender.Text;
                using (var context = new GoninDigitalDBContext())
                {
                    var productResult = context.Products.Where(product => product.Name.Contains(content))
                        .Select(product => new SearchItem {
                            Id = product.Id,
                            Name = product.Name,
                            Description = product.Description,
                            Image = product.Image,
                            Type = SearchItem.ItemType.PRODUCT
                        })
                        .ToList();
                    var vendorResult = context.Vendors.Where(vendor => vendor.Name.Contains(content))
                        .Select(vendor => new SearchItem {
                            Id=vendor.Id,
                            Name = vendor.Name,
                            Description = vendor.Description,
                            Image = vendor.Avatar,
                            Type=SearchItem.ItemType.VENDOR
                        })
                        .ToList();


                    var combined = vendorResult.Concat(productResult);
                    if (combined.Any())
                        sender.ItemsSource = combined;
                    else
                        sender.ItemsSource = new List<SearchItem>() 
                        { new SearchItem { Name = "No Results Found", Image = null, Description = null } };
                }
            }
        }

        private void AutoSuggestBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            if (args.ChosenSuggestion != null)
            {
                SearchItem searchItem = (SearchItem)args.ChosenSuggestion;

                if (searchItem.Type == SearchItem.ItemType.PRODUCT)
                {
                    RootFrame.Navigate(new ProductPage(searchItem.Id));
                    
                }
                else if (searchItem.Type == SearchItem.ItemType.VENDOR)
                {
                    // implement navigate to vendor
                }
                navigationView.IsPaneOpen = false;
            }
            else
            {
                // Use args.QueryText to determine what to do.
                //MessageBox.Show((string)args.QueryText);
            }
            
        }

        private void ListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            var selection = (e.ClickedItem as ListViewItem).Name;
            if (selection == "accountInfo")
            {
                RootFrame.Navigate(new UserPage());
            }
            else if (selection == "logout")
            {
                // clear
                Settings.Default.usrname = "";
                Settings.Default.passwod = "";

                //var loginWindow = new LoginViewModel(Application.Current.MainWindow);
                WindowManager.ChangeWindowContent(Application.Current.MainWindow, Properties.Resources.LoginWindowTitle, Properties.Resources.LoginControlPath);
            }
        }
    }
}
