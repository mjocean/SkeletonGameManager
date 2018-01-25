﻿using System;
using System.Windows.Input;
using Prism.Events;
using SkeletonGameManager.WPF.Interfaces;
using Prism.Commands;
using SkeletonGameManager.WPF.Model;
using SkeletonGameManager.WPF.Events;
using GongSolutions.Wpf.DragDrop;
using System.Windows;
using System.Linq;
using System.IO;
using System.Collections.Generic;

namespace SkeletonGameManager.WPF.ViewModels
{
    [PropertyChanged.AddINotifyPropertyChangedInterface]
    public class SceneMediaViewModel : SkeletonGameManagerViewModelBase, IDropTarget
    {
        public IMediaPlayer _mediaElement;

        #region Commands
        public ICommand MarkVideoRangeCommand { get; set; }
        public DelegateCommand<IMediaPlayer> MediaElementLoadedCommand { get; set; }
        public ICommand VideoControlCommand { get; set; }
        public ICommand AddToProcessListCommand { get; set; }
        #endregion

        public SceneMediaViewModel(IEventAggregator eventAggregator) : base(eventAggregator)
        {

            MarkVideoRangeCommand = new DelegateCommand<string>(MarkVideoRange);
            MediaElementLoadedCommand = new DelegateCommand<IMediaPlayer>(MediaElementLoaded);
            VideoControlCommand = new DelegateCommand<string>(OnVideoControl);

            _eventAggregator.GetEvent<VideoSourceEvent>().Subscribe(OnVideoSourceUpdated);

            //Add the selection start and end to process list
            AddToProcessListCommand = new DelegateCommand(AddToProcessList);
        }

        #region Properties
        private string videoPreviewHeader;
        public string VideoPreviewHeader
        {
            get { return videoPreviewHeader; }
            set { SetProperty(ref videoPreviewHeader, value); }
        }

        public TimeSpan SliderValue { get; set; }

        private TimeSpan? selectionEnd;
        public TimeSpan? SelectionEnd
        {
            get { return selectionEnd; }
            set {
                SetProperty(ref selectionEnd, value);

                if (value != null)
                    _mediaElement.SetPosition((TimeSpan)value);
            }
        }

        private TimeSpan? selectionStart;
        public TimeSpan? SelectionStart
        {
            get { return selectionStart; }
            set {
                SetProperty(ref selectionStart, value);

                if (value != null)
                    _mediaElement.SetPosition((TimeSpan)value);
            }
        }

        private string videoSource;
        public string VideoSource
        {
            get { return videoSource; }
            set { SetProperty(ref videoSource, value); }
        }
        #endregion

        #region Public Methods

        public void DragOver(IDropInfo dropInfo)
        {
            try
            {
                //Needs a few checks here. We can be dragging in from explorer or across to the datagrid.
                var dragFileList = dropInfo.Data;

                //Dragged from windows
                if (dragFileList.GetType() == typeof(DataObject))
                {
                    var windowsFileList = ((DataObject)dropInfo.Data).GetFileDropList().Cast<string>();

                    dropInfo.Effects = windowsFileList.Any(item =>
                    {
                        var extension = Path.GetExtension(item);
                        return extension != null;
                    }) ? DragDropEffects.Copy : DragDropEffects.None;
                }
                else
                {
                    dropInfo.Effects = DragDropEffects.Copy;
                }

            }
            catch (System.Exception)
            {
            }
        }

        public void Drop(IDropInfo dropInfo)
        {
            List<string> droppedFiles = new List<string>();

            //Needs a few checks here. We can be dragging in from explorer or across to the datagrid.
            var dragFileList = dropInfo.Data;

            //Dragged files from windows
            if (dragFileList.GetType() == typeof(DataObject))
            {
                var windowsFileList = ((DataObject)dropInfo.Data).GetFileDropList().Cast<string>();

                droppedFiles.AddRange(windowsFileList);

                VideoSource = droppedFiles[0];
                _eventAggregator.GetEvent<VideoSourceEvent>().Publish(VideoSource);
            }
        }

        #endregion

        #region Support Methods

        private void AddToProcessList()
        {
            if (SelectionStart.HasValue && SelectionEnd.HasValue)
            {
                var trimVideoItem = new TrimVideo
                {
                    Start = SelectionStart.Value,
                    End = SelectionEnd.Value,
                    File = VideoSource
                };

                _eventAggregator.GetEvent<VideoProcessItemAddedEvent>()
                    .Publish(trimVideoItem);

                SelectionStart = null; SelectionEnd = null;
            }
        }

        /// <summary>
        /// Called when the Loaded event happens on the a MediaElement
        /// </summary>
        /// <param name="mediaPlayer">The media player.</param>
        private void MediaElementLoaded(IMediaPlayer mediaPlayer)
        {            
            if (mediaPlayer != null)
            {
                _mediaElement = mediaPlayer;
                SliderValue = _mediaElement.GetCurrentTime();                
            }
                
        }

        /// <summary>
        /// Called when a button is fired in the view
        /// </summary>
        /// <param name="obj">The object.</param>
        private void OnVideoControl(string obj)
        {
            switch (obj)
            {
                case "pause":
                    _mediaElement.Pause();
                    break;
                case "play":
                    _mediaElement.Play();
                    break;
                case "stop":
                    _mediaElement.Stop();
                    break;
                default:
                    break;
            }
        }

        private void OnVideoSourceUpdated(string obj)
        {
            VideoSource = obj;

            VideoPreviewHeader = obj;
        }

        private void MarkVideoRange(string inOut)
        {
            try
            {
                if (inOut == "In")
                {
                    SelectionStart = _mediaElement.GetCurrentTime();                    
                    if (SelectionStart > SelectionEnd)
                        SelectionEnd = null;
                }                    
                else if (inOut == "Out")
                {
                    SelectionEnd = _mediaElement.GetCurrentTime();                    
                    if (SelectionEnd < SelectionStart)
                        SelectionStart = null;
                }
                    
            }
            catch { }
        }
        #endregion
    }

}
