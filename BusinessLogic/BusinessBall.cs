﻿//____________________________________________________________________________________________________________________________________
//
//  Copyright (C) 2024, Mariusz Postol LODZ POLAND.
//
//  To be in touch join the community by pressing the `Watch` button and get started commenting using the discussion panel at
//
//  https://github.com/mpostol/TP/discussions/182
//
//_____________________________________________________________________________________________________________________________________

namespace TP.ConcurrentProgramming.BusinessLogic
{
    internal class Ball : IBall
    {
        public Ball(Data.IBall ball)
        {
            _dataBall = ball;
            _dataBall.NewPositionNotification += RaisePositionChangeEvent;
        }

        #region IBall
        public event EventHandler<IPosition>? NewPositionNotification;
        public double Radius => _dataBall.Radius; // Przekazujemy promień z warstwy danych
        public double Mass => _dataBall.Mass;
        #endregion

        #region private
        private readonly Data.IBall _dataBall;

        private void RaisePositionChangeEvent(object? sender, Data.IVector e)
        {
            NewPositionNotification?.Invoke(this, new Position(e.x, e.y));
        }
        #endregion
    }
}