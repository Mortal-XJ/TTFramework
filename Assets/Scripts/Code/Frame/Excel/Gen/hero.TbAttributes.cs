
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using Luban;


namespace cfg.hero
{
public partial class TbAttributes
{
    private readonly System.Collections.Generic.Dictionary<int, HeroAttributes> _dataMap;
    private readonly System.Collections.Generic.List<HeroAttributes> _dataList;
    
    public TbAttributes(ByteBuf _buf)
    {
        _dataMap = new System.Collections.Generic.Dictionary<int, HeroAttributes>();
        _dataList = new System.Collections.Generic.List<HeroAttributes>();
        
        for(int n = _buf.ReadSize() ; n > 0 ; --n)
        {
            HeroAttributes _v;
            _v = HeroAttributes.DeserializeHeroAttributes(_buf);
            _dataList.Add(_v);
            _dataMap.Add(_v.Id, _v);
        }
    }

    public System.Collections.Generic.Dictionary<int, HeroAttributes> DataMap => _dataMap;
    public System.Collections.Generic.List<HeroAttributes> DataList => _dataList;

    public HeroAttributes GetOrDefault(int key) => _dataMap.TryGetValue(key, out var v) ? v : null;
    public HeroAttributes Get(int key) => _dataMap[key];
    public HeroAttributes this[int key] => _dataMap[key];

    public void ResolveRef(Tables tables)
    {
        foreach(var _v in _dataList)
        {
            _v.ResolveRef(tables);
        }
    }

}

}