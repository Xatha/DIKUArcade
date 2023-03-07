namespace TestDIKUArcade.RenderTextTest;

using DIKUArcade;
using DIKUArcade.GUI;
using DIKUArcade.Graphics;
using DIKUArcade.Math;

public class Game : DIKUGame {

    private Text text;
    public Game(WindowArgs windowArgs) : base(windowArgs) {
        window.SetClearColor(new Vec3F(1, 1, 1)); //TODO: REMOVE
        text = new Text("MIN TEXT", new Vec2F(0.25f, 0.25f), new Vec2F(0.25f, 0.25f));
        text.SetColor(255, 255, 0, 0);
        text.GetShape().Rotation = (float)System.Math.PI / -3.0f;
        text.GetShape().ScaleXFromCenter(3.2f);
    }


    public override void Render() {
        text.SetColor(255, 255, 0, 0);
        text.RenderText();
    }

    public override void Update() { }
}