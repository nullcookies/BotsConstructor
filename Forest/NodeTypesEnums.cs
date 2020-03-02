namespace Forest
{
    enum NodeType
    {
        Unknown = 0,
        Root = 1,
        Info = 2,
        Section = 3,
        Product = 4,
        Input = 5,
        SendOrder = 6
    }

    enum CollectionType
    {
        Unknown = 0,
        Block = 1,
        Flipper = 2
    }

    enum DisplayType
    {
        Unknown = 0,
        Simple = 1,
        Multi = 2
    }

    enum InputType
    {
        Unknown = 0,
        Text = 1,
        Time = 2,
        Image = 3,
        Audio = 4,
        Video = 5,
        Document = 6
    }
}