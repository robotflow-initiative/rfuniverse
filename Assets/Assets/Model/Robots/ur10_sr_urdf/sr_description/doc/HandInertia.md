Author Guillaume WALCK gwalck@techfak.uni-bielefeld.de

work done during the HANDLE project in 2011 (inertia tensor moved to joint axis)
reworked 2015 (inertia tensor at COM)

Overview
========

Hand inertia estimated from bounding box or bounding cylinders.
mass estimated from volume of bounding boxes times density of material
  (sometimes adjusted to match real values)
computation done with octave


Octave functions
================

```octave
function [ix,iy,iz] = inertiabox(sx,sy,sz,mass)

ix = 1/12.0*mass*(sy*sy+sz*sz);
iy = 1/12.0*mass*(sx*sx+sz*sz);
iz = 1/12.0*mass*(sy*sy+sx*sx);
end
```

```octave
function [ix,iy,iz] = inertiacyl(radius,length,mass)
% along z

iz = mass* radius*radius/2;
ix = 1/12.0* mass * (3*radius*radius+length*length);
iy = ix;
end
```

Inertia tensors
===============

density delrin = 1.41g/cm3
aluminum density = 2.7g/cm3

```octave
wrist aluminum V=16+37.7/2 = 41cm3 m=100g
inertiabox(0.030,0.066,0.058,0.1)
Ix =    6.4333e-05
Iy =    3.5533e-05
Iz =    4.3800e-05
```

```octave
palm delrin V=200cm3 m=300g
inertiabox(0.085,0.020,0.118,0.30)
Ix =    3.5810e-04
Iy =    5.2872e-04
Iz =    1.9063e-04
```

```octave
lfmetacarpal V=13cm3 m=30g
inertiabox(0.035,0.022,0.073,0.030)
Ix =    1.4532e-05
Iy =    1.6385e-05
Iz =    4.2725e-06
```

```octave
knuckle aluminum V=3cm3  m=8g
inertiacyl(0.009,0.012,0.008)
Ix =    2.5800e-07
Iy =    2.5800e-07
Iz =    3.2400e-07
```

```octave
proximal delrin V=21cm3 m=30g
inertiabox(0.02,0.018,0.06,0.030)
Ix =    9.8100e-06
Iy =    1.0000e-05
Iz =    1.8100e-06
```

```octave
middle delrin V=12.24cm3 m = 17g
inertiabox(0.018,0.017,0.04,0.017)
Ix =    2.6761e-06
Iy =    2.7257e-06
Iz =    8.6842e-07
```

```octave
distal delrin + rubber V=7cm3 m=12g
 inertiabox(0.018,0.0145,0.027,0.012)
Ix =    9.3925e-07
Iy =    1.0530e-06
Iz =    5.3425e-07
```

```octave
thdistal delrin + rubber V=13cm3 m=16g
inertiabox(0.021,0.018,0.035,0.016)
Ix =    2.0653e-06
Iy =    2.2213e-06
Iz =    1.0200e-06
```

```octave
thproximal delrin V=29.5cm3  m=40g
inertiacyl(0.025/2,0.06,0.04)
Ix =    1.3562e-05
Iy =    1.3562e-05
Iz =    3.1250e-06
```

```octave
thmiddle delrin V=19.77cm3 m=27g
inertiacyl(0.022/2,0.052,0.027)
Ix =    6.9007e-06
Iy =    6.9007e-06
Iz =    1.6335e-06
```

```octave
thhub  m=5g
inertiabox(0.005,0.005,0.005,0.005)
Ix =    2.0833e-08
Iy =    2.0833e-08
Iz =    2.0833e-08
```

```octave
thbase  m=10g
inertiabox(0.010,0.010,0.010,0.010)
Ix =    1.6667e-07
Iy =    1.6667e-07
Iz =    1.6667e-07
```
