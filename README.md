# Zombie Survial

_Engine Test_

> [!NOTE]
> For now, this is not a game only an engine demo

# Scenes

## Loading Demo Scene

```cmd
ZombieSurvial demo
```

When getting an error check:

1. Does the scene folder have any malformed text
2. Is this `resource\scene\` directories is created
3. Forgot to add the `SaveNode` attribute, or uses the wrong name
4. Make an issue on the github project

## Loading Scenes

```cmd
ZombieSurvial load path-to-scene
```

When recieving an error, do the same steps in the [Loading Demo Scene](#loading-demo-scene).

## Scene Formating

> [!WARNING]
> Comments in scene files are not supported yet.
> [!NOTE]
> Editing a file scene directly is not suggested

### Node should always come first

A node declearation should be the first thing in scene, with all of the properties under it.

e.g.

```scene
// Node
[engine.camera local-id='eb73e2cfc53b45aca89fed0ecc236166' parent='00000000000000000000000000000000']
// Properties of Node
	ZombieSurvival.Engine.Vector3 Position={"X":0,"Y":0,"Z":0}
	ZombieSurvival.Engine.Vector3 Rotation={"X":0,"Y":0,"Z":0}
	ZombieSurvival.Engine.Vector3 Scale={"X":1,"Y":1,"Z":1}
	System.String Name="PlayerCamera"
```

### Type Name=Value

Node declearation properties should be as follows:

- Type: as in full name (so `float` becomes `System.Single`) and be or inherit the type in the node class.
- Name: The name should also be in a valid name, in the node's type
- Value: in the JSON format (allows for number literals like `NaN` or infinity)

### local-ids should not duplicate

Local id's should be unique and should not be duplicate in multiple Node declearations. For example:

```scene
[engine.node local-id"guid-here" parent="parent"] // fine
[engine.node local-id"guid-here" parent="parent"] // results in error, or unknown behaviour
```

### No parent is 32 0s

To declear that node is parented to tree, add 32 0s.

```scene
[engine.node local-id"guid-here" parent="00000000000000000000000000000000"]
```
