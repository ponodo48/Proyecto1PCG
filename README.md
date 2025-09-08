# Proyecto1PCG
Proyecto PCG Unidad 1

Para poder utilizar el proyecto, clonar este repositorio y añadir la carpeta ProyectoU1 en Unity 6.0
Si se desea poder editar desde el ciclo, cabe destacar la poca optimización de la regeneración del L_system
Utilidades del proyecto:
Cuenta con generación procedural de un terreno con algoritmo DiamondSquare, de un arbol con L-System y con un set de habitaciones con BSP optimizado.
Los Scripts son respectivamente Fractal.cs, L_System.cs y BSP.cs
Tiene varios problemas de diseño.
Además cuenta con un package de 50 materials PBR para ir cambiando dentro de los objetos de la escena. (Solo con fines educacionales)

DiamondSquare y BSP necesitan estar ligados a un Plane y un Quad respectivamente. L-System necesita un Cylinder de modelo para poder funcionar, además de una clase serializable instanciada por inspector, donde deben estar inicializados predecessor donde se escribe la regla que se quiere cambiar, por un successor que es la regla por la que se cambiara y las probabilidades que si bien no estan implementadas en el código, es necesario crear al menos una con un valor alto para que funcione.

Sientete libre de manipular el código y utilizarlo para tus proyectos.
