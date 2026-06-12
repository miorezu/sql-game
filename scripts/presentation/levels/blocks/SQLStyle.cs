using Godot;



public static class SqlStyle
{
    public static Color GetColorForType(BlockType type)
    {
        return type switch
        {
            BlockType.Keyword  => Colors.SkyBlue,       
            BlockType.Table    => Colors.BlueViolet,     
            BlockType.Column   => Colors.LightGoldenrod, 
            BlockType.Operator => Colors.LightPink,        
            BlockType.Value    => Colors.White,         
            _                  => Colors.Gray
        };
    }
}