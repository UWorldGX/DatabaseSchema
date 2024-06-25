using FirewallDemo.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Intrinsics.Arm;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace FirewallDemo.View.Extended;

public class PwdBoxExtended
{
    public static readonly DependencyProperty PasswordProperty = DependencyProperty.RegisterAttached("Password", typeof(string), typeof(PwdBoxExtended),
                                                                                                           new FrameworkPropertyMetadata(string.Empty, OnPasswordPropertyChanged));

    public static readonly DependencyProperty AttachProperty = DependencyProperty.RegisterAttached("Attach", typeof(bool), typeof(PwdBoxExtended), new PropertyMetadata(false, OnAttachPropertyChanged));

    private static readonly DependencyProperty IsUpdatingProperty = DependencyProperty.RegisterAttached("IsUpdating", typeof(bool), typeof(PwdBoxExtended));

    public static void SetAttach(DependencyObject dp, bool value)
    {
        dp.SetValue(AttachProperty, value);
    }

    public static bool GetAttach(DependencyObject dp)
    {
        return (bool)dp.GetValue(AttachProperty);
    }

    public static string GetPassword(DependencyObject dp)
    {
        return (string)dp.GetValue(PasswordProperty);
    }

    public static void SetPassword(DependencyObject dp, string value)
    {
        dp.SetValue(PasswordProperty, value);
    }

    private static bool GetIsUpdating(DependencyObject dp)
    {
        return (bool)dp.GetValue(IsUpdatingProperty);
    }

    private static void SetIsUpdating(DependencyObject dp, bool value)
    {
        dp.SetValue(IsUpdatingProperty, value);
    }

    private static void OnPasswordPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
    {
        if (sender is PasswordBox passwordBox)
        {
            passwordBox.PasswordChanged -= PasswordChanged;
            if (!(bool)GetIsUpdating(passwordBox))
            {
                passwordBox.Password = (string)e.NewValue;
            }
            passwordBox.PasswordChanged += PasswordChanged;
        }

    }

    private static void OnAttachPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
    {
        if (sender is not PasswordBox passwordBox)
        {
            return;
        }
        if ((bool)e.OldValue)
        {
            passwordBox.PasswordChanged -= PasswordChanged;
        }
        if ((bool)e.NewValue)
        {
            passwordBox.PasswordChanged += PasswordChanged;
        }
    }

    private static void PasswordChanged(object sender, RoutedEventArgs e)
    {
        if(sender is PasswordBox passwordBox)
        {
            SetIsUpdating(passwordBox, true);
            SetPassword(passwordBox, passwordBox.Password);
            SetIsUpdating(passwordBox, false);
        }

    }
}