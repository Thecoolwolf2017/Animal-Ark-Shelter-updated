# Scene and Behavior

This page groups scene dressing and behavior settings with their default values.

## Scene (Shelter Yard)
- SceneX / SceneY / SceneZ: 597.0833, 2800.7881, 41.3537
  - Fixed world anchor where the showcase and camera are placed.
- CameraOffsetX / CameraOffsetY / CameraOffsetZ: 4.5, 4.0, 1.6
  - Offset from SceneX/Y/Z for shop camera placement.
- FOV: 62.0
  - Wider angle to frame benches/customers.
- ShowcaseOffsetX / ShowcaseOffsetY / ShowcaseOffsetZ: 1.6, 1.2, 0.0
  - Offset from SceneX/Y/Z to spawn the showcase animal.
- ShopkeeperOffsetX / ShopkeeperOffsetY / ShopkeeperOffsetZ: -0.8, -0.5, 0.0
  - Clerk position near the counter.
- SpawnBench: true, BenchCount: 2
- SpawnCustomer: true, CustomerCount: 2
- SpawnDecor: true

## Behavior
- ComeHere
  - WarpIfFar: true
  - WarpDistance: 120.0
  - RunSpeed: 3.0
  - StopRange: 1.2
  - TeleportIfStuckMs: 4500
- Vehicle
  - SeatIndex: 0
  - Pose: 0 (0 = sit, 1 = lay)

Tip: Tweak offsets in `AnimalArkShelter.ini` to suit your map location. Scene defaults aim to frame the yard at the shelter location listed above.

