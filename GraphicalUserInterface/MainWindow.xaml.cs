﻿//__________________________________________________________________________________________
//
//  Copyright 2024 Mariusz Postol LODZ POLAND.
//
//  To be in touch join the community by pressing the `Watch` button and to get started
//  comment using the discussion panel at
//  https://github.com/mpostol/TP/discussions/182
//__________________________________________________________________________________________

using System;
using System.Windows;
using System.Windows.Controls;
using TP.ConcurrentProgramming.Presentation.ViewModel;

namespace TP.ConcurrentProgramming.PresentationView
{
  /// <summary>
  /// View implementation
  /// </summary>
  public partial class MainWindow : Window
  {
    public MainWindow()
    {
      Random random = new Random();
      InitializeComponent();
      //MainWindowViewModel viewModel = (MainWindowViewModel)DataContext;
      //double screenWidth = SystemParameters.PrimaryScreenWidth;
      //double screenHeight = SystemParameters.PrimaryScreenHeight;
      //viewModel.Start(random.Next(5, 10));
    }

    private void StartButton_Click(object sender, RoutedEventArgs e)
    {
        if (int.TryParse(BallCountTextBox.Text, out int ballCount) && ballCount > 0 && ballCount <= 50)
        {
            MainWindowViewModel viewModel = (MainWindowViewModel)DataContext;
            double screenWidth = SystemParameters.PrimaryScreenWidth;
            double screenHeight = SystemParameters.PrimaryScreenHeight;
                SetDynamicTableSize();
                viewModel.Start(ballCount, _tableWidth, _tableHeight);

            BallCountTextBox.Visibility = Visibility.Collapsed;
            ((Button)sender).Visibility = Visibility.Collapsed;
            BallCountTextBlock.Visibility = Visibility.Collapsed;
        }
        else
        {
            MessageBox.Show("Proszę wprowadzić prawidłową liczbę piłek.");
        }
    }

        /// <summary>
        /// Raises the <seealso cref="System.Windows.Window.Closed"/> event.
        /// </summary>
        /// <param name="e">An <see cref="EventArgs"/> that contains the event data.</param>
        protected override void OnClosed(EventArgs e)
    {
      if (DataContext is MainWindowViewModel viewModel)
        viewModel.Dispose();
      base.OnClosed(e);
    }

    private double _tableWidth;
    private double _tableHeight;

    private void SetDynamicTableSize()
    {
        double screenWidth = SystemParameters.PrimaryScreenWidth;
        double screenHeight = SystemParameters.PrimaryScreenHeight;

        _tableWidth = screenWidth * 0.7;
        _tableHeight = screenHeight * 0.7;

        GameTableBorder.Width = _tableWidth;
        GameTableBorder.Height = _tableHeight;
    }
    }
}