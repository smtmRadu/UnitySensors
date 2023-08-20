# Unity Sensors

Special components used to capture observations in a custom environment. Working both in 2D and 3D, they provide a simple to adjust interface in order to overlap the necessary environmental objects that need the be captured.
- RaySensor: Casts multiple rays in order to obtain information about close or far objects.
- GridSensor: Cast multiple boxes mapping the environmental objects in close proximity.
- CameraSensor: Captures an image of the environment (also takes manual captures, saved in Assets folder)

All observations are taken calling **GetObservationsVector()** method (or other particular **GetObservations()** method), that returns a float[] of size provided in the lower part of the component.

Use cases: robotics, reinforcement learning, game mechanics.

![image](https://github.com/smtmRadu/blob/UnitySensors/camerasensor.png)
![image2](https://github.com/smtmRadu/blob/UnitySensors/gridsensor.png)
![image3](https://github.com/smtmRadu/blob/UnitySensors/camerasensor.png)