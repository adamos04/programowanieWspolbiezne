//__________________________________________________________________________________________
//
//  Copyright 2024 Mariusz Postol LODZ POLAND.
//
//  To be in touch join the community by pressing the `Watch` button and to get started
//  comment using the discussion panel at
//  https://github.com/mpostol/TP/discussions/182
//__________________________________________________________________________________________

using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using TP.ConcurrentProgramming.Presentation.Model;
using TP.ConcurrentProgramming.Presentation.ViewModel.MVVMLight;
using ModelIBall = TP.ConcurrentProgramming.Presentation.Model.IBall;

namespace TP.ConcurrentProgramming.Presentation.ViewModel
{
  public class MainWindowViewModel : ViewModelBase, IDisposable
  {
    #region ctor

    public MainWindowViewModel() : this(null, null) { }

    public MainWindowViewModel(ModelAbstractApi modelLayerAPI, ScreenSizeProxy screenSize)
    {
        ModelLayer = modelLayerAPI == null ? ModelAbstractApi.CreateModel() : modelLayerAPI;
        Observer = ModelLayer.Subscribe<ModelIBall>(x => Balls.Add(x));
        StartCommand = new RelayCommand(StartMethod);
        ScreenSize = screenSize;
    }

        #endregion ctor

        #region public API

        public void Start(int numberOfBalls, double tableWidth, double tableHeight)
    {
      if (Disposed)
        throw new ObjectDisposedException(nameof(MainWindowViewModel));
      ModelLayer.Start(numberOfBalls, tableWidth, tableHeight);
      Observer.Dispose();
    }

    public ICommand StartCommand { get; }
    public ObservableCollection<ModelIBall> Balls { get; } = new ObservableCollection<ModelIBall>();

    public ScreenSizeProxy ScreenSize { get; set; }

    private string _ballInput;
    public string BallInput
    {
        get => _ballInput;
        set
        {
            _ballInput = value;
            RaisePropertyChanged(nameof(BallInput));
        }
    }

    private double _tableWidth;
    private double _tableHeight;

    public double TableWidth
    {
        get => _tableWidth;
        private set
        {
            _tableWidth = value;
            RaisePropertyChanged(nameof(TableWidth));
        }
    }

    public double TableHeight
    {
        get => _tableHeight;
        private set
        {
            _tableHeight = value;
            RaisePropertyChanged(nameof(TableHeight));
        }
    }

        private void StartMethod()
        {
            if (int.TryParse(BallInput, out int numberOfBalls) && numberOfBalls >= 1 && numberOfBalls <= 50)
            {
                TableWidth = ScreenSize.Width * 0.7;
                TableHeight = ScreenSize.Height * 0.7;
                Start(numberOfBalls, TableWidth, TableHeight);
            }
            else
            {
                //MessageBox.Show("Proszę wprowadzić prawidłową liczbę piłek (1-50).");
            }
        }

        #endregion public API

        #region IDisposable

        protected virtual void Dispose(bool disposing)
    {
      if (!Disposed)
      {
        if (disposing)
        {
          Balls.Clear();
          Observer.Dispose();
          ModelLayer.Dispose();
        }

        // TODO: free unmanaged resources (unmanaged objects) and override finalizer
        // TODO: set large fields to null
        Disposed = true;
      }
    }

    public void Dispose()
    {
      if (Disposed)
        throw new ObjectDisposedException(nameof(MainWindowViewModel));
      Dispose(disposing: true);
      GC.SuppressFinalize(this);
    }

    #endregion IDisposable

    #region private

    private IDisposable Observer = null;
    private ModelAbstractApi ModelLayer;
    private bool Disposed = false;

    #endregion private
  }
}