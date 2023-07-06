# Unity Sensors

Special components used to capture observations in a custom environment. Working both in 2D and 3D, they provide a simple to adjust interface in order to overlap the necessary environmental objects that need the be captured.
- RaySensor: Casts multiple rays in order to obtain information about close or far objects.
- GridSensor: Cast multiple boxes mapping the environmental objects in close proximity.
- CamSensor: Captures an image of the environment (also takes manual captures, saved in the Assets folder)

All observations are taken calling **GetObservartions()** method, that returns an IEnumerable variable. The length of this collection depends of the adjustments set for each sensor (the length can be computed easily by following the formula described on the method summary separately- hover over the method when calling).

Use cases: robotics, reinforcement learning, game mechanics.

![simage](https://github.com/RaduTM-spec/UnitySensors/assets/67599940/6c6bcf0a-8a0d-42bc-b266-615aa6f0a638)
![simage2](https://github.com/RaduTM-spec/UnitySensors/assets/67599940/74547ff1-b38b-4132-bf5a-0903b8f0a65a)
