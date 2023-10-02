using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace RoboDeck
{
    public partial class CardEditor : UserControl
    {
        bool mouseWasDown = false;
        public CardEditor()
        {
            InitializeComponent();
            meta.InitTimeline();
            meta.EditorMode = UserInterfaceMode.Editing;
        }

        private delegate void UpdateImageDelegate(Image image);
        private delegate IntPoint GetIntPointDelegate();
        private delegate void LoadDelegate();
        
        public void Redraw(double _deltaTime)
        {
            var mousePosition = GetCursorPositionOnControl();
            Redraw(mousePosition, _deltaTime);
        }

        MetaState meta = new MetaState()
        {
            TheDeck = new Deck() {
                Cards = new List<Card>()
                {
                    new Card(),
                },
            },

            TheStage = new Stage()
                {
                    Cells = new bool[,]{
                    {true, true, true, true, true, true,true},
                    {true,false,false,false,false,false,true},
                    {true,false,false,false,false,false,true},
                    {true,false,false,false,false,false,true},
                    {true,false,false,false,false,false,true},
                    {true,false,false,false,false,false,true},
                    {true, true, true, true, true, true,true},
                },
                    Entrance = new IntPoint(3, 5),
                    Exit = new IntPoint(3, 1),
                },
            EditorMode = UserInterfaceMode.Editing,
            HighlightedCard = -1,
            HighlightedRobo = -1,
        };
        DeckDrawer deckDrawer = new DeckDrawer(1000);
        TimelineDrawer timelineDrawer = new TimelineDrawer(1000);
        StageDrawer stageDrawer = new StageDrawer();
        LoaderButtonsDrawer stageLoaderButtons = new LoaderButtonsDrawer();
        LoaderButtonsDrawer deckLoaderButtons = new LoaderButtonsDrawer();

        public void Redraw(IntPoint _mousePosition, double _deltaTime)
        {
            var size = GetImageSize();
            if(size.X <= 0 || size.Y <= 0)
            {
                return;
            }
            var request = LoaderRequest.None;
            var parameters =new DrawingParameters(_mousePosition, 2);
            var bigParameters = new DrawingParameters(_mousePosition, 3);

            deckDrawer.Width = size.X - parameters.BigGridStep * 3;
            timelineDrawer.Width = size.X - parameters.BigGridStep * 3;
            var stageDrawerPosition = new IntPoint(parameters.BigGridStep * 2, parameters.BigGridStep);
            var timelineDrawerPosition = new IntPoint(parameters.BigGridStep * 2, stageDrawer.GetHeight(parameters, meta.TheStage) + stageDrawerPosition.Y + parameters.CardOffset);
            var deckDrawerPosition = new IntPoint(parameters.BigGridStep * 2, timelineDrawerPosition.Y + (parameters.BigGridStep + parameters.CardOffset) * 2);
            var roboMarkerPosition = new IntPoint(stageDrawerPosition.X + stageDrawer.GetWidth(parameters, meta.TheStage), stageDrawerPosition.Y);
            meta.ResetHighlighting();
            RoboMarkerDrawer.Update(parameters, meta, roboMarkerPosition);
            timelineDrawer.Update(parameters, meta, timelineDrawerPosition, _deltaTime);
            stageDrawer.Update(parameters, meta, stageDrawerPosition);
            deckDrawer.Update(bigParameters, meta, _deltaTime, deckDrawerPosition);
            stageLoaderButtons.Update();
            deckLoaderButtons.Update();

            var mouseIsDown = (Control.MouseButtons & System.Windows.Forms.MouseButtons.Left) != 0 && GameForm.AllowCursorControl;
            if (mouseIsDown && !mouseWasDown)
            {
                timelineDrawer.MouseDown(parameters, meta, timelineDrawerPosition);
                if (meta.EditorMode == UserInterfaceMode.Editing)
                {
                    request = stageLoaderButtons.MouseDown(parameters, stageDrawerPosition, LoaderRequest.Stage);
                }
                if (request == LoaderRequest.None)
                {
                    request = deckLoaderButtons.MouseDown(parameters, deckDrawerPosition, LoaderRequest.Deck);
                }
                deckDrawer.MouseDown(bigParameters, meta, deckDrawerPosition);
                stageDrawer.MouseDown(parameters, meta, stageDrawerPosition);
            }
            if (!mouseIsDown && mouseWasDown)
            {
                deckDrawer.MouseUp();
                timelineDrawer.MouseUp(meta.TheTimeline);
                stageDrawer.MouseUp();
            }
            mouseWasDown = mouseIsDown;

            var image = new Bitmap(size.X, size.Y);
            var graphics = Graphics.FromImage(image);

            stageDrawer.DrawStage(graphics, parameters, stageDrawerPosition, meta);
            deckDrawer.DrawDeck(graphics, bigParameters, meta, deckDrawerPosition);
            timelineDrawer.DrawTimeline(graphics, parameters, meta, timelineDrawerPosition);
            stageLoaderButtons.DrawButtons(graphics, parameters, stageDrawerPosition, meta.EditorMode != UserInterfaceMode.Editing);
            deckLoaderButtons.DrawButtons(graphics, parameters, deckDrawerPosition, false);
            RoboMarkerDrawer.DrawRoboMarkers(graphics, parameters, meta, roboMarkerPosition);
            
            UpdateImage(image);
            LoadData(request);
            GC.Collect();
        }

        private void LoadData(LoaderRequest _request)
        {
            switch(_request)
            {
                case LoaderRequest.LoadStage:
                    LoadStage();
                    return;
                case LoaderRequest.SaveStage:
                    SaveStage();
                    return;
                case LoaderRequest.LoadDeck:
                    LoadDeck();
                    return;
                case LoaderRequest.SaveDeck:
                    SaveDeck();
                    return;
            }
        }

        private void LoadStage()
        {
            if (this.InvokeRequired)
            {
                var delegatedLoad = new LoadDelegate(this.DoLoadStage);
                try
                {
                    this.Invoke(delegatedLoad);
                }
                catch { }
            }
            else
            {
                DoLoadStage();
            }
        }

        private void SaveStage()
        {
            if (this.InvokeRequired)
            {
                var delegatedLoad = new LoadDelegate(this.DoSaveStage);
                try
                {
                    this.Invoke(delegatedLoad);
                }
                catch { }
            }
            else
            {
                DoSaveStage();
            }
        }

        private void LoadDeck()
        {
            if (this.InvokeRequired)
            {
                var delegatedLoad = new LoadDelegate(this.DoLoadDeck);
                try
                {
                    this.Invoke(delegatedLoad);
                }
                catch { }
            }
            else
            {
                DoLoadDeck();
            }
        }

        private void SaveDeck()
        {
            if (this.InvokeRequired)
            {
                var delegatedLoad = new LoadDelegate(this.DoSaveDeck);
                try
                {
                    this.Invoke(delegatedLoad);
                }
                catch { }
            }
            else
            {
                DoSaveDeck();
            }
        }

        private void UpdateImage(Bitmap image)
        {
            if (this.InvokeRequired)
            {
                var delegatedUpdateImage = new UpdateImageDelegate(this.DoUpdateImage);
                try
                {
                    this.Invoke(delegatedUpdateImage, image);
                }
                catch { }
            }
            else
            {
                DoUpdateImage(image);
            }
        }

        private IntPoint GetImageSize()
        {
            if (this.InvokeRequired)
            {
                var delegatedGetImageSize = new GetIntPointDelegate(this.DoGetImageSize);
                try
                {
                    return (IntPoint)this.Invoke(delegatedGetImageSize);
                }
                catch { return IntPoint.DownRight; }
            }
            else
            {
                return DoGetImageSize();
            }
        }

        private IntPoint GetCursorPositionOnControl()
        {
            if (this.InvokeRequired)
            {
                var delegatedGetImageSize = new GetIntPointDelegate(this.DoGetCursorPositionOnControl);
                try
                {
                    return (IntPoint)this.Invoke(delegatedGetImageSize);
                }
                catch { return IntPoint.DownRight; }
            }
            else
            {
                return DoGetCursorPositionOnControl();
            }
        }

        private static String GetPathToLocalDirectory()
        {
            return Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
        }

        private static String BuildPath(string folder)
        {
            var path = Path.Combine(GetPathToLocalDirectory(), folder);
            if (!Directory.Exists(path))
            {
                try
                {
                    Directory.CreateDirectory(path);
                }
                catch { }
            }
            return path;
        }

        private static String PathToLevels;

        private static String GetPathToLevels()
        {
            if (String.IsNullOrEmpty(PathToLevels))
            {
                var folder = "Levels";
                PathToLevels = BuildPath(folder);
            }
            return PathToLevels;
        }

        private static String PathToSolutions;

        private static String GetPathToSolutions()
        {
            if (String.IsNullOrEmpty(PathToSolutions))
            {
                var folder = "Solutions";
                PathToSolutions = BuildPath(folder);
            }
            return PathToSolutions;
        }

        private void DoLoadStage()
        {
            using (var openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "txt|*.txt";
                openFileDialog.InitialDirectory = GetPathToLevels();
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    PathToLevels = Path.GetDirectoryName(openFileDialog.FileName);
                    try
                    {
                        var stage = Loader.LoadStage(openFileDialog.FileName);
                        GameForm.SetName(Path.GetFileNameWithoutExtension(openFileDialog.FileName));
                        meta.TheStage = stage;
                    }
                    catch { }
                }
            }
        }

        private void DoSaveStage()
        {
            using (var saveFileDialog = new SaveFileDialog())
            {
                saveFileDialog.Filter = "txt|*.txt";
                saveFileDialog.InitialDirectory = GetPathToLevels();
                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    PathToLevels = Path.GetDirectoryName(saveFileDialog.FileName);
                    try
                    {
                        Loader.SaveStage(meta.TheStage, saveFileDialog.FileName);
                    }
                    catch { }
                }
            }
        }

        private void DoLoadDeck()
        {
            using (var openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "txt|*.txt";
                openFileDialog.InitialDirectory = GetPathToSolutions();
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    PathToSolutions = Path.GetDirectoryName(openFileDialog.FileName);
                    try
                    {
                        var deck = Loader.LoadDeck(openFileDialog.FileName);
                        meta.TheDeck = deck;
                        meta.OnDeckEdited();
                    }
                    catch { }
                }
            }
        }

        private void DoSaveDeck()
        {
            using (var saveFileDialog = new SaveFileDialog())
            {
                saveFileDialog.Filter = "txt|*.txt";
                saveFileDialog.InitialDirectory = GetPathToSolutions();
                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    PathToSolutions = Path.GetDirectoryName(saveFileDialog.FileName);
                    try
                    {
                        Loader.SaveDeck(meta.TheDeck, saveFileDialog.FileName);
                    }
                    catch { }
                }
            }
        }

        public void DoUpdateImage(Image image)
        {
            var previousImage = this.pictureBox.Image;
            this.pictureBox.Image = image;
            if (previousImage != null)
                previousImage.Dispose();
        }

        public IntPoint DoGetImageSize()
        {
            return new IntPoint(
                this.pictureBox.Size.Width,
                this.pictureBox.Size.Height
                );
        }

        public IntPoint DoGetCursorPositionOnControl()
        {
            return (IntPoint)(this.PointToClient(Cursor.Position));
        }
    }
}
