# Malgar Hotel (Skeleton)

## Requisitos
- Unity 2022.3 LTS (recomendado) o Unity 2021.3 LTS
- Unity Hub para gestionar la apertura del proyecto

## Cómo abrir el proyecto
1. Clona el repositorio y asegúrate de tener `git lfs` instalado (`git lfs install`).
2. Desde Unity Hub, selecciona **Add project from disk** y elige la carpeta raíz de este repositorio.
3. Abre el proyecto con la versión LTS indicada.

## Estructura principal
- `Assets/`
  - `Scenes/`: escenas `Main` y `Lobby` agregadas a *Build Settings*.
  - `Scripts/`: stubs organizados por dominio (`Core`, `World`, `Player`, etc.).
  - `Prefabs/`, `UI/`, `ScriptableObjects/`: preparados para futuros assets.
  - `Settings/URP/`: configuración básica del Universal Render Pipeline.
- `Packages/`: dependencias y paquetes configurados (URP incluido).
- `ProjectSettings/`: configuración del proyecto (Color Space en Linear, URP asignado, escenas en build).

## Próximos pasos sugeridos
- Implementar la lógica de *bootstrap* y control de jugador.
- Integrar assets reales (modelos, UI y audio) siguiendo la estructura existente.
- Configurar *post-processing* y perfiles específicos por escena según las necesidades del juego.
- Añadir pruebas automatizadas y herramientas de editor que faciliten el flujo de trabajo.
