using System;
using System.Numerics;
using System.Collections.Generic;
using Raylib_cs;
using MetroidvaniaRuntime;
using InputHelper;
using MathExtras;
using TextureAtlases;
using MetroidvaniaLevels;
using UndoRedo;

namespace MapEditor
{
    public class EditorWindow : BaseWindow, IBehavedWindow
    {
        //References
        public TileSelectWindow tileSelector;
        public DecoSelectWindow decoSelector;
        public EditorSelectionHandler selectionHandler;

        private Level EditorLevel => selectionHandler.currentLevel;
        private Room SelectedRoom => selectionHandler.currentRoom;
        private Room.AbstractLayer targetLayer = null;

        //Editing
        private bool isDrawingTiles = false;
        private bool editingRoomLayout = false;
        private int selectedRoomEdge = -1; //-1: none, 0: left, 1: right, 2: top, 3: bottom, 4 5 6 7: NW NE SW SE corners
        private int dragOriginX = 0, dragOriginY = 0;
        private int translateOriginX = 0, translateOriginY = 0;
        private bool draggingEdge = false;

        private EditorManager.WindowMode EditMode => EditorManager.RWindowMode;
        private Vector2 decoPreviewPos = Vector2.Zero;
        private Room.Decoration selectedDecoration = null;
        private Room.Decoration hoveredDecoration = null;

        private readonly ActionHistory actionHistory = new ActionHistory();
        private IAction currentAction = null;
        private bool LockUndoRedo => Input.Held_AMB;

        //Visuals
        private Camera2D editorCamera;
        private float TextureScale => Screen.pixelScale;
        public static Color gridColor = new Color(200, 200, 200, 20);

        //Methods
        public void RunWindowBehaviour()
        {
            if (!LockUndoRedo) HandleUndoRedo();
            HandleCameraMovement();
            HandleMouseInteraction();

            BeginDrawing();
            ClearBackground();
            Raylib.BeginMode2D(editorCamera);
            
            foreach (Room room in EditorLevel.RoomDictionary.Values)
            {
                if (room != SelectedRoom)
                {
                    room.DrawInEditor(editorCamera, selected: false);
                }
            }
            SelectedRoom.DrawInEditor(editorCamera, drawGrid: editorCamera.zoom > 0.5f, selected: true);
            if (EditMode == EditorManager.WindowMode.DecoSelect && decoSelector.selectedSprite != -1 && isMouseOver) DrawDecoPreview();

            DrawMainAxes();
            if (selectedDecoration != null) HighlightDecoObject(selectedDecoration, true);
            if (hoveredDecoration != null) HighlightDecoObject(hoveredDecoration, true);

            Raylib.EndMode2D();
            Raylib.DrawFPS(5, 5);
            EndDrawing();
            DrawToScreen();
        }

        private void HandleCameraMovement()
        {
            UpdateMouseState();
            if (isMouseOver)
            {
                float scrollAmount = Raylib.GetMouseWheelMove();
                if (scrollAmount > 0 && editorCamera.zoom < 100)
                {
                    editorCamera.zoom *= 1.2f;
                    Console.WriteLine($"Camera zoom = {editorCamera.zoom}");
                }
                else if (scrollAmount < 0 && editorCamera.zoom > -100)
                {
                    editorCamera.zoom /= 1.2f;
                    Console.WriteLine($"Camera zoom = {editorCamera.zoom}");
                }
                if (Input.Held_MMB)
                {
                    editorCamera.target -= MouseDeltaPosition / editorCamera.zoom;
                }
            }
        }
        private void HandleUndoRedo()
        {
            if (Input.Held_LCTRL)
            {
                if (Raylib.IsKeyPressed(KeyboardKey.KEY_Z))
                {
                    if (Input.Held_LSHIFT) actionHistory.RedoNextAction();
                    else actionHistory.UndoLastAction();
                }
                else if (Raylib.IsKeyPressed(KeyboardKey.KEY_Y))
                {
                    actionHistory.RedoNextAction();
                }
            }
        }
        private void HandleMouseInteraction()
        {
            if (EditMode != EditorManager.WindowMode.Selection)
            {
                hoveredDecoration = null;
                selectedDecoration = null;
            }
            if (isMouseOver)
            {
                Vector2 mouseWorldPos = GetMouseLevelPos();
                if (Input.Clicked_LMB && !IsMouseOverRoom(mouseWorldPos, SelectedRoom))
                {
                    FindAndSwitchRoom(mouseWorldPos);
                    targetLayer = null;
                    selectedDecoration = null;
                }

                if (Input.Held_LALT)
                {
                    editingRoomLayout = true;
                    HandleLayoutEditing();
                }
                else
                {
                    editingRoomLayout = false;
                    Raylib.SetMouseCursor(MouseCursor.MOUSE_CURSOR_DEFAULT);
                    if (EditMode == EditorManager.WindowMode.TileSelect)
                    {
                        HandleTilePainting();
                    }
                    else if (EditMode == EditorManager.WindowMode.DecoSelect)
                    {
                        HandleDecoEditing();
                    }
                    else if (EditMode == EditorManager.WindowMode.Selection)
                    {
                        HandleSelection();
                    }
                }
            }
        }
        private void HandleTilePainting()
        {
            Vector2 mouseWorldPos = GetMouseLevelPos();

            int mouseTileX = (int)mouseWorldPos.X - (mouseWorldPos.X < 0 ? 1 : 0);
            int mouseTileY = (int)mouseWorldPos.Y - (mouseWorldPos.Y < 0 ? 1 : 0);

            //LMB Clicked
            if (Input.Clicked_LMB)
            {
                isDrawingTiles = true;

                if (!SelectedRoom.HasLayerOfType((int)Room.LayerType.Tiling))
                {
                    SelectedRoom.AddLayer(new Room.TilingLayer(SelectedRoom));
                }
                targetLayer = SelectedRoom.GetLayerOfType((int)Room.LayerType.Tiling);

                currentAction = new Room.TilingLayer.TileChangeAction(targetLayer as Room.TilingLayer);
                (currentAction as Room.TilingLayer.TileChangeAction).before = (targetLayer as Room.TilingLayer).GetSavestate();
            }
            //LMB Held
            if (Input.Held_LMB)
            {
                if (isDrawingTiles && IsMouseOverRoom(mouseWorldPos, SelectedRoom))
                {
                    (targetLayer as Room.TilingLayer).tiles[mouseTileX - SelectedRoom.RoomGlobalPosX, mouseTileY - SelectedRoom.RoomGlobalPosY] = (byte)tileSelector.selectedTile;
                }
            }
            //LMB Released
            if (Input.Released_LMB)
            {
                if (isDrawingTiles)
                {
                    isDrawingTiles = false;

                    (currentAction as Room.TilingLayer.TileChangeAction).after = (targetLayer as Room.TilingLayer).GetSavestate();
                    actionHistory.RecordAction(currentAction);
                }
            }
        }
        private void HandleLayoutEditing()
        {
            Vector2 mouseWorldPos = GetMouseLevelPos();
            int mousePosX = mouseWorldPos.X.ToNearestInteger();
            int mousePosY = mouseWorldPos.Y.ToNearestInteger();
            float edgeHandleWidth = 0.3f / editorCamera.zoom;

            //Mouse state
            int hoveredEdge = selectedRoomEdge;
            bool dragRoom = false;
            if (!draggingEdge)
            {
                #region Calculating hovered edge and setting mouse handle
                if (Math.Abs(mouseWorldPos.X - SelectedRoom.RoomGlobalPosX) < edgeHandleWidth)
                {
                    hoveredEdge = 0;
                }
                else if (Math.Abs(mouseWorldPos.X - (SelectedRoom.RoomGlobalPosX + SelectedRoom.RoomWidth)) < edgeHandleWidth)
                {
                    hoveredEdge = 1;
                }

                if (Math.Abs(mouseWorldPos.Y - SelectedRoom.RoomGlobalPosY) < edgeHandleWidth)
                {
                    switch (hoveredEdge)
                    {
                        case 0:
                            hoveredEdge = 4;
                            Raylib.SetMouseCursor(MouseCursor.MOUSE_CURSOR_RESIZE_NWSE);
                            break;
                        case 1:
                            hoveredEdge = 5;
                            Raylib.SetMouseCursor(MouseCursor.MOUSE_CURSOR_RESIZE_NESW);
                            break;
                        default:
                            hoveredEdge = 2;
                            Raylib.SetMouseCursor(MouseCursor.MOUSE_CURSOR_RESIZE_NS);
                            break;
                    }
                }
                if (Math.Abs(mouseWorldPos.Y - (SelectedRoom.RoomGlobalPosY + SelectedRoom.RoomHeight)) < edgeHandleWidth)
                {
                    switch (hoveredEdge)
                    {
                        case 0:
                            hoveredEdge = 6;
                            Raylib.SetMouseCursor(MouseCursor.MOUSE_CURSOR_RESIZE_NESW);
                            break;
                        case 1:
                            hoveredEdge = 7;
                            Raylib.SetMouseCursor(MouseCursor.MOUSE_CURSOR_RESIZE_NWSE);
                            break;
                        default:
                            hoveredEdge = 3;
                            Raylib.SetMouseCursor(MouseCursor.MOUSE_CURSOR_RESIZE_NS);
                            break;
                    }
                }
                
                if (hoveredEdge == 0 || hoveredEdge == 1) Raylib.SetMouseCursor(MouseCursor.MOUSE_CURSOR_RESIZE_EW);

                if (hoveredEdge == -1)
                {
                    if (IsMouseOverRoom(mouseWorldPos, SelectedRoom) || dragRoom == true)
                    {
                        dragRoom = true;
                        Raylib.SetMouseCursor(MouseCursor.MOUSE_CURSOR_RESIZE_ALL);
                    }
                    else
                    {
                        Raylib.SetMouseCursor(MouseCursor.MOUSE_CURSOR_DEFAULT);
                    }
                }
                #endregion
            }

            if (Input.Clicked_LMB)
            {
                selectedRoomEdge = hoveredEdge;
                if (dragRoom)
                {
                    dragOriginX = mousePosX;
                    dragOriginY = mousePosY;
                    translateOriginX = mousePosX;
                    translateOriginY = mousePosY;
                }
            }
            if (Input.Held_LMB)
            {
                if (false && selectedRoomEdge != -1)
                {
                    draggingEdge = true;
                    if (selectedRoomEdge < 4)
                    {
                        TrySetRoomEdge(selectedRoomEdge, (selectedRoomEdge == 0 || selectedRoomEdge == 1) ? mousePosX : mousePosY);
                    }
                    else
                    {
                        //Dragging corners (two edges at once)
                        switch (selectedRoomEdge)
                        {
                            case 4: // NW corner
                                {
                                    TrySetRoomEdge(0, mousePosX);
                                    TrySetRoomEdge(2, mousePosY);
                                    break;
                                }
                            case 5: // NE corner
                                {
                                    TrySetRoomEdge(1, mousePosX);
                                    TrySetRoomEdge(2, mousePosY);
                                    break;
                                }
                            case 6: // SW corner
                                {
                                    TrySetRoomEdge(0, mousePosX);
                                    TrySetRoomEdge(3, mousePosY);
                                    break;
                                }
                            case 7: // SE corner
                                {
                                    TrySetRoomEdge(1, mousePosX);
                                    TrySetRoomEdge(3, mousePosY);
                                    break;
                                }
                        }
                    }
                }
                if (dragRoom)
                {
                    if (mousePosX != dragOriginX || mousePosY != dragOriginY)
                    {
                        SelectedRoom.Translate(mousePosX - dragOriginX, mousePosY - dragOriginY);
                        dragOriginX = mousePosX;
                        dragOriginY = mousePosY;
                    }
                }
            }
            if (Input.Released_LMB)
            {
                if (selectedRoomEdge != -1)
                {
                    draggingEdge = true;
                    if (selectedRoomEdge < 4)
                    {
                        TrySetRoomEdge(selectedRoomEdge, (selectedRoomEdge == 0 || selectedRoomEdge == 1) ? mousePosX : mousePosY);
                    }
                    else
                    {
                        TrySetRoomCorner(selectedRoomEdge, mousePosX, mousePosY);
                    }
                }
                if (dragRoom)
                {
                    if (mousePosX != translateOriginX || mousePosY != translateOriginY)
                    {
                        SelectedRoom.Translate(mousePosX - dragOriginX, mousePosY - dragOriginY);
                        Room.RoomTranslateAction action = new Room.RoomTranslateAction(SelectedRoom, mousePosX - translateOriginX, mousePosY - translateOriginY);
                        actionHistory.RecordAction(action);
                    }
                }
                selectedRoomEdge = -1;
                draggingEdge = false;
            }
        }
        private void HandleDecoEditing()
        {
            if (SelectedRoom.HasLayerOfType((int)Room.LayerType.Deco))
            {
                targetLayer = SelectedRoom.GetLayerOfType((int)Room.LayerType.Deco);
            }
            if (Input.Clicked_RMB) decoSelector.selectedSprite = -1; //deselect

            if (decoSelector.selectedSprite != -1)
            {
                HandleDecoPlacement();
            }
        }
        private void HandleDecoPlacement()
        {
            SpriteInfo sprite = decoSelector.GetCurrentAtlas().Sprites[decoSelector.selectedSprite];
            decoPreviewPos = (GetMouseLevelPosUnscaled() - new Vector2(sprite.Width / 2, sprite.Height / 2)).RoundXY();
            if (!Input.Held_LCTRL)
            {
                decoPreviewPos -= new Vector2(sprite.OffsetX, sprite.OffsetY);
                float mx = decoPreviewPos.X % 16, my = decoPreviewPos.Y % 16;
                decoPreviewPos += new Vector2(mx < 8 ? -mx : 16 - mx, my < 8 ? -my : 16 - my);
                decoPreviewPos += new Vector2(sprite.OffsetX, sprite.OffsetY);
            }
            if (Input.Clicked_LMB)
            {
                if (targetLayer == null || targetLayer.GetLayerTypeId() != (int)Room.LayerType.Deco)
                {
                    if (!SelectedRoom.HasLayerOfType((int)Room.LayerType.Deco))
                    {
                        SelectedRoom.AddLayer(new Room.DecoLayer(SelectedRoom));
                    }
                    targetLayer = SelectedRoom.GetLayerOfType((int)Room.LayerType.Deco);
                }
                Room.DecoLayer decoLayer = targetLayer as Room.DecoLayer;

                TextureAtlas atlas = decoSelector.GetCurrentAtlas();
                TextureAtlas newAtlas = EditorLevel.TexManager.RequestAtlas(atlas.PathInDirectory);
                decoLayer.AddDecoration(newAtlas, atlas.Sprites[decoSelector.selectedSprite].Id, (int)decoPreviewPos.X - (SelectedRoom.RoomGlobalPosX * 16), (int)decoPreviewPos.Y - (SelectedRoom.RoomGlobalPosY * 16), out Room.Decoration d);
                actionHistory.RecordAction(new Room.DecoLayer.AoRDAction(targetLayer as Room.DecoLayer, d, true));
            }
        }
        private void HandleSelection()
        {
            HandleDecoSelection();
        }
        private void HandleDecoSelection()
        {
            if (SelectedRoom.HasLayerOfType((int)Room.LayerType.Deco))
            {
                targetLayer = SelectedRoom.GetLayerOfType((int)Room.LayerType.Deco);
            }

            hoveredDecoration = FindDecoAtMouse();
            if (Input.Clicked_LMB)
            {
                selectedDecoration = hoveredDecoration != null ? hoveredDecoration : null;
            }
            if (selectedDecoration != null)
            {
                if (Raylib.IsKeyPressed(KeyboardKey.KEY_BACKSPACE) || Raylib.IsKeyPressed(KeyboardKey.KEY_DELETE))
                {
                    Room.DecoLayer layer = targetLayer as Room.DecoLayer;
                    Room.DecoLayer.AoRDAction action = new Room.DecoLayer.AoRDAction(layer, selectedDecoration, false);
                    actionHistory.RecordAction(action);
                    layer.RemoveDecoration(selectedDecoration);
                    selectedDecoration = null;
                }
            }
        }

        private void TrySetRoomEdge(int edge, int newValue)
        {
            Room.RoomTransformAction action = null;

            switch (edge)
            {
                case 0: //left
                    {
                        int difference = newValue - SelectedRoom.RoomGlobalPosX;
                        difference = Math.Min(difference, SelectedRoom.RoomWidth - 2);
                        action = new Room.RoomTransformAction(SelectedRoom, difference, 0, -difference, 0);
                        break;
                    }
                case 1: //right
                    {
                        int newWidth = newValue - SelectedRoom.RoomGlobalPosX;
                        newWidth = Math.Max(newWidth, 2);
                        action = new Room.RoomTransformAction(SelectedRoom, 0, 0, newWidth - SelectedRoom.RoomWidth, 0);
                        break;
                    }
                case 2: //top
                    {
                        int difference = newValue - SelectedRoom.RoomGlobalPosY;
                        difference = Math.Min(difference, SelectedRoom.RoomHeight - 2);
                        action = new Room.RoomTransformAction(SelectedRoom, 0, difference, 0, -difference);
                        break;
                    }
                case 3: //bottom
                    {
                        int newHeight = newValue - SelectedRoom.RoomGlobalPosY;
                        newHeight = Math.Max(newHeight, 2);
                        action = new Room.RoomTransformAction(SelectedRoom, 0, 0, 0, newHeight - SelectedRoom.RoomHeight);
                        break;
                    }
            }

            if (action.dx != 0 || action.dy != 0 || action.dw != 0 || action.dh != 0)
                actionHistory.RecordAndExectute(action);
        }
        private void TrySetRoomCorner(int corner, int newX, int newY)
        {
            bool westCorner = corner == 4 || corner == 6;
            bool northCorner = corner == 4 || corner == 5;

            int dx;
            if (westCorner)
            {
                dx = newX - SelectedRoom.RoomGlobalPosX;
                dx = Math.Min(dx, SelectedRoom.RoomWidth - 2);
            }
            else
            {
                dx = newX - SelectedRoom.RoomGlobalPosX - SelectedRoom.RoomWidth;
                dx = Math.Max(dx, 2 - SelectedRoom.RoomWidth);
            }

            int dy;
            if (northCorner)
            {
                dy = newY - SelectedRoom.RoomGlobalPosY;
                dy = Math.Min(dy, SelectedRoom.RoomHeight - 2);
            }
            else
            {
                dy = newY - SelectedRoom.RoomGlobalPosY - SelectedRoom.RoomHeight;
                dy = Math.Max(dy, 2 - SelectedRoom.RoomHeight);
            }

            Room.RoomTransformAction action = new Room.RoomTransformAction(SelectedRoom, westCorner ? dx : 0, northCorner ? dy : 0, westCorner ? -dx : dx, northCorner ? -dy : dy);

            if (action.dx != 0 || action.dy != 0 || action.dw != 0 || action.dh != 0)
                actionHistory.RecordAndExectute(action);
        }
        private void FindAndSwitchRoom(Vector2 mouseWorldPos)
        {
            foreach (KeyValuePair<string, MetroidvaniaLevels.Room> kvp in EditorLevel.RoomDictionary)
            {
                //Switching selected room
                if (IsMouseOverRoom(mouseWorldPos, kvp.Value))
                {
                    selectionHandler.currentRoom = kvp.Value;
                    break;
                }
            }
            //do nothing if no room is found*
        }
        private Room.Decoration FindDecoAtMouse()
        {
            if (targetLayer is Room.DecoLayer decoLayer)
            {
                Vector2 mousePos = GetMouseLevelPosUnscaled();
                int mousePosX = (int)mousePos.X - (SelectedRoom.RoomGlobalPosX * 16);
                int mousePosY = (int)mousePos.Y - (SelectedRoom.RoomGlobalPosY * 16);

                List<Room.Decoration> decos = decoLayer.GetDecorationList();
                for (int i = 0; i < decos.Count; i++)
                {
                    if (IsMouseOverDecoration(decos[i], mousePosX, mousePosY))
                    {
                        return decos[i];
                    }
                }
                return null;
            }
            else return null;
        }
        private bool IsMouseOverDecoration(Room.Decoration d, int mousePosX, int mousePosY)
        {
            return !(d.X > mousePosX || d.Y > mousePosY || d.X + d.width < mousePosX || d.Y + d.height < mousePosY);
        }

        private void DrawDecoPreview()
        {
            Rectangle spriteRec = decoSelector.GetCurrentAtlas().GetSpriteRec(decoSelector.selectedSprite);
            Rectangle destRec = new Rectangle(decoPreviewPos.X, decoPreviewPos.Y, spriteRec.width, spriteRec.height).Multiply(Screen.pixelScale);
            Raylib.DrawTexturePro(decoSelector.GetCurrentAtlas().Texture, spriteRec,destRec, Vector2.Zero, 0, Color.WHITE);
        }
        private void DrawMainAxes()
        {
            Raylib.DrawLine(-int.MaxValue, 0, int.MaxValue, 0, gridColor);
            Raylib.DrawLine(0, -int.MaxValue, 0, int.MaxValue, gridColor);
        }
        private void DrawDebugDots()
        {
            for (int x = 0; x < 10; x++)
            {
                Raylib.DrawCircle(x*Screen.pixelScale*16, 0, 5, Color.RED);
            }
            for (int y = 0; y < 10; y++)
            {
                Raylib.DrawCircle(0, y * Screen.pixelScale * 16, 5, Color.RED);
            }
        }
        private void HighlightAllDecoObjects()
        {
            if (targetLayer is Room.DecoLayer deco)
            {
                foreach (Room.Decoration d in deco.GetDecorationList())
                {
                    HighlightDecoObject(d, false);
                }
            }
        }
        private void HighlightDecoObject(Room.Decoration d, bool hover)
        {
            int s = Screen.pixelScale;
            int xo = 16 * SelectedRoom.RoomGlobalPosX;
            int yo = 16 * SelectedRoom.RoomGlobalPosY;
            int x1 = s * (d.X + xo);
            int x2 = s * (d.X + d.width + xo);
            int y1 = s * (d.Y + yo);
            int y2 = s * (d.Y + d.height + yo);
            Color c = new Color(gridColor.r, gridColor.g, gridColor.b, gridColor.a * 3);

            Raylib.DrawLine(x1, y1, x2, y1, c);
            Raylib.DrawLine(x1, y1, x1, y2, c);
            Raylib.DrawLine(x2, y1, x2, y2, c);
            Raylib.DrawLine(x1, y2, x2, y2, c);
        }

        //Functions
        private bool IsMouseOverRoom(Vector2 mouseLevelPos, Room room)
        {
            int mouseTileX = (int)mouseLevelPos.X;
            int mouseTileY = (int)mouseLevelPos.Y;
            if (mouseLevelPos.X < 0) mouseTileX -= 1;
            if (mouseLevelPos.Y < 0) mouseTileY -= 1;

            return (!(mouseTileX < room.RoomGlobalPosX || mouseTileX >= room.RoomGlobalPosX + room.RoomWidth
                || mouseTileY < room.RoomGlobalPosY || mouseTileY >= room.RoomGlobalPosY + room.RoomHeight));
        }
        private Vector2 GetMouseLevelPos()
        {
            return GetMouseLevelPosUnscaled() / 16;
        }
        private Vector2 GetMouseLevelPosUnscaled()
        {
            return Raylib.GetScreenToWorld2D(mouseCurrentPosition - new Vector2(windowScreenX, windowScreenY), editorCamera) / (TextureScale);
        }

        //Constructor
        public EditorWindow(EditorSelectionHandler selectionHandler)
        {
            windowScreenX = EditorManager.leftMenuWidth;
            windowScreenY = 0;
            windowWidth = EditorManager.screenWidth - EditorManager.rightMenuWidth - EditorManager.leftMenuWidth;
            windowHeight = EditorManager.screenHeight;
            windowBackgroundColor = new Color(50, 50, 50, 255);
            renderTexture = Raylib.LoadRenderTexture(windowWidth, windowHeight);
            editorCamera = new Camera2D { offset = new Vector2(windowWidth / 2, windowHeight / 2), target = new Vector2(windowWidth / 2, windowHeight / 2), zoom = 1f };
            this.selectionHandler = selectionHandler;
        }
    }
}