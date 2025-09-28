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

## Núcleo singleplayer disponible
- Escena **Main** lista para jugar: presiona *Play* y muévete con WASD.
- **Jugador FPS** con `CharacterController`: sprint (Shift), agacharse (Ctrl), linterna (F) y `Interact` (E).
- **Linterna con batería**: drenaje configurable, *flicker* bajo 20% y recarga mediante `BatteryPickup`.
- **Objetivo eléctrico**: recolecta/repara 5 fusibles para activar el ascensor y volver al lobby.
- **Generador procedural**: pasillos rectos, curvas y salas reciclando tiles con *pooling*.
- **UI**: HUD de batería/fusibles con barra visible, menú de pausa con `Esc` (Resume / Options / Quit) y pantalla de victoria.
- **Opciones en pausa**: sliders para sensibilidad del ratón, volúmenes Master/SFX/Ambience/UI con persistencia en `PlayerPrefs`.
- **Audio**: *room tone* procedural, pasos con randomización de tono/volumen y ruteo a un `AudioMixer` básico.

## Próximos pasos sugeridos
- Sustituir placeholders por assets definitivos (modelos low-poly, efectos de sonido y UI).
- Ampliar el minijuego de reparación y conectar el `NoiseSystem` con la futura IA.
- Añadir validaciones y pruebas automatizadas para los sistemas principales.
