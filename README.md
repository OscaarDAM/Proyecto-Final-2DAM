# Proyecto Final DAM - Videojuego

Este proyecto empezó como una idea para mi propuesta del trabajo final, la cual fue aceptada tanto por el profesorado como por la empresa donde actualmente estoy haciendo prácticas, llamada **Distribuciones Batoy**.

Como idea principal, pensamos que hacer una serie de videojuegos podría funcionar. Empecé buscando ideas y mecánicas para dichos minijuegos, e incluso llegué a hacer uno parecido al clásico *Donkey Kong*.

A la hora de hacer la primera reunión para decidir el futuro de mi juego, concluimos que los minijuegos no eran lo que buscábamos y decidimos centrarnos en algo que pudiera mantenerse y expandirse a futuro. Así, acabamos decidiendo hacer un **juego procedural de mazmorras para móvil**.

Cuando ya teníamos la idea clara, empecé a investigar cómo podría generar mazmorras y niveles aleatorios, y cómo podría, dentro de estas mazmorras, generar tanto objetos como enemigos... y por supuesto, al jugador.

---

## Avances del videojuego hasta el 29/04/2025

Los primeros días estuve investigando y haciendo pruebas sobre la generación de salas y mazmorras aleatorias. Cuando lo conseguí, empecé a buscar e implementar pasillos que conectaran dichas salas y que tuvieran consistencia.

Una vez logrado, lo mejoré añadiendo colisiones a los muros y creé el personaje principal con sus propias colisiones y controles mediante joystick.

Con la base del juego lista, decidí hacer un menú provisional e investigar cómo hacer transiciones más suaves entre escenas.

Con esto montado, pasé a crear los enemigos. Antes de eso, decidí almacenar la sala donde se genera el jugador para usos futuros. Al acabar, programé la generación de enemigos por sala (de momento, 3 enemigos por sala aleatoriamente) y creé un área invisible por sala que, al entrar en contacto con el jugador, activa el evento que hace que los enemigos lo ataquen.

Como último paso hasta ahora, configuré que, además de seguir al jugador, al entrar en una sala aparezcan unos bloques que impiden salir hasta eliminar a todos los enemigos (momentáneamente, esto se controla con el ratón, ya que aún estamos en fase de pruebas).

---

## Implementaciones actuales

- Menú  
- Transiciones de menú a nivel  
- Barra de vida del jugador  
- Enemigos que siguen al jugador al entrar a la sala  
- Puertas que se abren al entrar a la sala  
- Generación procedural de niveles
- Pantalla de muerte y pantalla de pausa
- Reemplazar el sprite estático de suelo y paredes por un array de sprites colocados aleatoriamente
- Añadir una escena final
- Pasar de nivel conservando estadísticas, incluyendo la vida restante
- Minimapa para la dungeon y el coliseo
- Nuevo nivel "Coliseo"
- Cambio de tiles de paredes simulando "limpieza del nivel"
- Interfaz de pausa y joysticks configurados y diseñados"
- Crear objetos de curación
- Crear diferentes tipos de mecánicas de ataque para los enemigos  
- Crear animaciones de movimiento y ataque

---

## Futuras implementaciones

- Quitar vida a enemigos con armas     
- Crear armas para el juego  
- Crear efectos visuales y de sonido  

