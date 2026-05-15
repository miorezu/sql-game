using Godot;



public static class SqlStyle
{
    public static Color GetColorForType(BlockType type)
    {
        return type switch
        {
            BlockType.Keyword  => Colors.SkyBlue,       
            BlockType.Table    => Colors.PaleGreen,     
            BlockType.Column   => Colors.LightGoldenrod, 
            BlockType.Operator => Colors.Tomato,        
            BlockType.Value    => Colors.White,         
            _                  => Colors.Gray
        };
    }
}