﻿//____________________________________________________________________________________________________________________________________
//
//  Copyright (C) 2023, Mariusz Postol LODZ POLAND.
//
//  To be in touch join the community by pressing the `Watch` button and get started commenting using the discussion panel at
//
//  https://github.com/mpostol/TP/discussions/182
//
//  by introducing yourself and telling us what you do with this community.
//_____________________________________________________________________________________________________________________________________

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using TP.ConcurrentProgramming.BusinessLogic;
using LogicIBall = TP.ConcurrentProgramming.BusinessLogic.IBall;

namespace TP.ConcurrentProgramming.Presentation.Model
{
  internal class ModelBall : IBall
  {
    public ModelBall(double top, double left, LogicIBall underneathBall)
    {
      TopBackingField = top;
      LeftBackingField = left;
      _mass = underneathBall.Mass;
      _radius = underneathBall.Radius;
      underneathBall.NewPositionNotification += NewPositionNotification;
    }

    #region IBall

    public double Top
    {
      get { return TopBackingField; }
      private set
      {
        if (TopBackingField == value)
          return;
        TopBackingField = value;
        RaisePropertyChanged();
      }
    }

    public double Left
    {
      get { return LeftBackingField; }
      private set
      {
        if (LeftBackingField == value)
          return;
        LeftBackingField = value;
        RaisePropertyChanged();
      }
    }

    public double Diameter { get; init; } = 0;
    public double Radius => _radius;
    public double Mass => _mass;
    public string Color => GetColorForMass(_mass);

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

    #endregion INotifyPropertyChanged

    #endregion IBall

    #region private

    private double TopBackingField;
    private double LeftBackingField;
    private readonly double _mass;
    private readonly double _radius;

        private void NewPositionNotification(object sender, IPosition e)
    {
            Top = e.y - Radius; 
            Left = e.x - Radius;
        }

    private void RaisePropertyChanged([CallerMemberName] string propertyName = "")
    {
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

        private string GetColorForMass(double mass)
        {
            if (mass < 3 || mass > 6)
                throw new ArgumentOutOfRangeException("Mass must be between 3 and 6");

            int red = (int)(255 - 35 * (mass - 3));
            int greenBlue = (int)(200 * (1 - (mass - 3) / 3));
            string hexColor = $"#{red:X2}{greenBlue:X2}{greenBlue:X2}";

            return hexColor;
        }

        #endregion private

        #region testing instrumentation

        [Conditional("DEBUG")]
    internal void SetLeft(double x)
    { Left = x; }

    [Conditional("DEBUG")]
    internal void SettTop(double x)
    { Top = x; }

    #endregion testing instrumentation
  }
}