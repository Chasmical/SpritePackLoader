# SpritePackLoader

<div align="center">
  <p>
    <a href="https://github.com/Abbysssal/SpritePackLoader/releases/latest">
      <img src="https://img.shields.io/github/v/release/Abbysssal/SpritePackLoader?label=Latest%20release&style=for-the-badge&logo=github" alt="Latest release"/>
    </a>
    <a href="https://github.com/Abbysssal/SpritePackLoader/releases">
      <img src="https://img.shields.io/github/v/release/Abbysssal/SpritePackLoader?include_prereleases&label=Latest%20pre-release&style=for-the-badge&logo=github" alt="Latest pre-release"/>
    </a>
    <br/>
    <a href="https://github.com/Abbysssal/SpritePackLoader/releases">
      <img src="https://img.shields.io/github/downloads/Abbysssal/SpritePackLoader/total?label=Downloads&style=for-the-badge" alt="Downloads"/>
    </a>
    <a href="https://github.com/Abbysssal/SpritePackLoader/subscription">
      <img src="https://img.shields.io/github/watchers/Abbysssal/SpritePackLoader?color=green&label=Watchers&style=for-the-badge" alt="Watchers"/>
    </a>
    <a href="https://github.com/Abbysssal/SpritePackLoader/stargazers">
      <img src="https://img.shields.io/github/stars/Abbysssal/SpritePackLoader?color=green&label=Stars&style=for-the-badge" alt="Stars"/>
    </a>
  </p>
</div>

## Installing SpritePackLoader

**[Follow the instructions on RogueLibs' site](https://abbysssal.github.io/RogueLibs/docs/user/installation)**.

## Installing SpritePacks

1. Install BepInEx, RogueLibs and SpritePackLoader *(obviously)*;
2. Download your .spritepack file that you want to use;
3. Put it in /BepInEx/spritepacks directory (create one if it doesn't exist);
4. Launch the game!

## Creating Your Own SpritePack

Alright, it might not be as easy as it could possibly be, but it's the best I could do. SoR's code is a mess. Especially with those weird TK2D sprites, that I spent months trying to figure out.
1. All sprites must be PNG- or JPEG-encoded, and square.
2. All sprites' file names must be their ids in the game (case-sensitive).
3. Go to your game's root directory and create `SpritePack` folder, if it doesn't already exist. And create folders called `ITEMS`, `OBJECTS` and etc. in it.
4. If your sprites are not 64x64, add _SIZExSIZE to the directory names. Example: `ITEMS_64x64`. You can have multiple directories with the same scope.

Any of the modifiers above (size, scope) also can be applied to files. Scope modifier is put before the sprite id, and the size - after. Examples: `Blindenizer_8x8.png` (in ITEMS directory), `ITEMS_BearTrap.png` (doesn't matter what directory the file is in), `ITEMS_Beer_512x512` (btw the extension also doesn't matter, just make sure the data itself is PNG- or JPEG-encoded).

Here's some examples of valid identifiers:
- `ITEMS/Blindenizer.png` (that is, a file called `Blindenizer.png` in a folder called `ITEMS`);
- `OBJECTS_AmmoDispenser.png`;
- `ITEMS_16x16/BaseballBat.png`;
- `ITEMS/Beer_256x256.png`;
- `ITEMS_Chainsaw_1024x1024.png`;

5. Launch the game, and all of the sprites from the `SpritePack` directory will be loaded, as if they are in a .spritepack file. It's great for seeing how your sprites look in the game, without having to pack all of it in a .spritepack file.
   - If you need to test item sprites, you can use a SpritePack Tester item in the Item Teleporter's menu, that will spawn all item sprites around the player.
6. To publish a spritepack, put the directories in a .zip archive, then change the file's extension to .spritepack and upload it!

#### More details

Scope identifiers must be in the beginning of the path part (path parts are identifiers, separated by `/`, excluding the extension). So, both `ITEMS` and `ITEMS_my-items` are valid. Just make sure you separate the identifier from the rest with `_` (underscore).

Currently available scopes:
- `ITEMS` - items and abilities;
- `OBJECTS` - objects;
- `FLOORS` - floor tiles;
- `BULLETS` - bullets and other projectiles;
- `HAIR` - hair;
- `FACIALHAIR` - facial hair;
- `HEADPIECES` - head pieces;
- `AGENTS` - eyes, heads, arms and legs;
- `BODIES` - bodies;
- `WRECKAGE` - wreckage, that spawns after destroying something;
- `INTERFACE` - interface stuff;
- `DECALS` - decals;
- `WALLTOPS` - tops of the walls;
- `WALLS` - sides of the walls;
- `SPAWNERS` - icons in level editor;
- `EXTRA` - extra sprites, don't worry about that;

Size identifiers must be in the end of the path part. Both `512x512` and `my-items_512x512` are valid. They are separated with a `_` (underscore) as well.

The sprite's name is extracted from the last path part, excluding all other identifiers, so it doesn't matter what directories that you put it in are called. You can have `ITEMS_MyCoolSprites/bla-blabla/BooUrn.png`, and only `BooUrn` will be recognized.

