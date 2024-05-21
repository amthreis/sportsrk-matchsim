FROM amthreis/godot-mono-builder:4.2.2 AS build

WORKDIR /app

COPY . .
RUN mkdir dist && godot --headless --path . --export-release "Linux/X11" dist/game.x86_64

FROM ubuntu

WORKDIR /app

ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT 1
ENV DOCKER 1

COPY --from=build /app/dist/ /app/

CMD ./game.x86_64 --headless