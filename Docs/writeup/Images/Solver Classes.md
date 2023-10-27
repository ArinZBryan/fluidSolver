```plantuml
class FluidEffect {
solver

func1()
func2()
}

FluidEffect::solver <|-- Solver

interface Solver 

class Solver2D implements Solver {
SWAP<T>()
add_source()
set_bnd()
lin_solve()
diffuse()
advect()
project()
dens_step()
vel_step()
}

class Solver3D implements Solver {
SWAP<T>()
add_source()
set_bnd()
lin_solve()
diffuse()
advect()
project()
dens_step()
vel_step()
}

enum Boundary2D {
NONE
HORIZONTAL
VERTICAL
}

```
