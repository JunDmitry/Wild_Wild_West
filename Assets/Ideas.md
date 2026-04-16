# Ideas

---

## Model

```
public interface IModel
{
	int Id { get; }
	ReadOnlyDictionary<int, IReadOnlyComponentData> StaticData { get; }
	Dictionary<int, ComponentData> State { get; }
}

public class BaseModel : IModel
{
	public BaseModel(int id, ComponentData[] startComponents)
	{
		Id = id;
		StaticData = startComponents
			.ToDictionary(c => c.GetTypeId(), c => c);
		State = startComponents
			.ToDictionary(c => c.GetTypeId(), c => c);
	}
	
	int Id { get; }
	ReadOnlyDictionary<int, IReadOnlyComponentData> StaticData { get; }
	Dictionary<int, ComponentData> State { get; }
}
```

---

## Presenter

```
public interface IPresenter 
{
	void Show();
	void Hide();
}

public abstract class BasePresenter<TModel, TData> : IPresenter
	where TModel : IModel
{
	private bool _isShow;
	private IModel _model;
	private IModelView<TData> _view;

	public BasePresenter(IModel model, IModelView<TData> view)
	{
		_model = model;
		_view = view;
	}

	public void Show()
	{
		if (_isShow)
			return;

		_isShow = true;

		OnShowing();
		_view.Show();
		OnShowed();
	}

	public void Hide()
	{
		if (_isShow == false)
			return;

		_isShow = false;

		OnHiding();
		_view.Hide();
		OnHided();
	}

	protected virtual void OnShowing()
	{ }

	protected virtual void OnShowed()
	{ }

	protected virtual void OnHiding()
	{ }

	protected virtual void OnHided()
	{ }
}
```

---

## View

```
public interface IView 
{
	void Show();
	void Hide();
}

public interface IView<TData> : IView
{
	void Update(TData data);
}

public abstract class BaseView<TData> : MonoBehaviour, IView<TData>
{
	public sealed void Show()
	{
		OnShowing();
		gameObject.SetActive(true);
		OnShowed();
	}

	public sealed void Hide()
	{
		OnHiding();
		gameObject.SetActive(false);
		OnHided();
	}

	public abstract void Update(TData data);

	protected virtual void OnShowing()
	{ }

	protected virtual void OnShowed()
	{ }

	protected virtual void OnHiding()
	{ }

	protected virtual void OnHided()
	{ }
}
```

---

## ComponentData for configs

```
public interface IReadOnlyComponentData
{
	int GetTypeId();
}

[Serializable]
public abstract class ComponentData : IReadOnlyComponentData
{
	public readonly static int TypeId = GetType().GetId();

	public int GetTypeId()
	{
		return TypeId;
	}

	public abstract ComponentData CloneDeep();
}

```

---

> **Todo**
>
> Tries make path selector attribute with valid prefabs
> for set ComponentData

---

## Tags for GameObject

```
public abstract class ComponentTag : MonoBehaviour
{
	public abstract readonly static IReadOnlyList<int> Requirement;
}

public static class TypeIdentifier
{
	private static Dictionary<Type, int> s_idByType = new();
	private static int s_id = 1;

	public static int GetId(this Type type) 
	{
		if (s_idByType.TryGetValue(type, out int id) == false)
		{
			id = s_id++;
			s_idByType[type] = id;
		}

		return s_idByType[type];
	}
}
```

---

## Example
```

```