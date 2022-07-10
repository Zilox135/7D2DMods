using System.Xml;

public class HitTransformCompare : TargetedCompareRequirementBase
{
	private string hitTransformName;

	public override bool IsValid(MinEventParams _params)
	{
		if (!base.IsValid(_params))
		{
			return false;
		}

		if (Voxel.voxelRayHitInfo.hitCollider != null)
        {
			if (Voxel.voxelRayHitInfo?.Clone().hitCollider.transform.name == hitTransformName && _params.Other.IsInFrontOfMe(_params.Self.position))
            {
				return true;
			}
        }
		return false;
	}

	public override bool ParseXmlAttribute(XmlAttribute _attribute)
	{
		bool flag = base.ParseXmlAttribute(_attribute);
		if (!flag)
		{
			string name = _attribute.Name;
			if (name != null)
			{
				if (name == "hit_transform")
				{
					this.hitTransformName = _attribute.Value;
					return true;
				}
			}
		}
		return flag;
	}
}
