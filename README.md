# SimpleSpritePacker
SimpleSpritePacker generate sprite atlas from sprites


**How to use**

1. Install or build and run application
2. Add sprites to input files list (drag drop or button)

 (you can delete with 'D' when items are selected)

![ssp_screen_01](https://user-images.githubusercontent.com/79088221/139769957-0f23b62e-a13c-49a0-866f-9c895c393128.PNG)

3. [Optional] Sort added input files (files will be placed within atlas in order from inpty file list)
4. [Optional] Mark the checkbox "Geneate additional ...", this is required if you want to use auto rename sprite slice tools for Unity Editor
5. Set output path
6. Click 'Generate' button
7. Files Sprite atlas png will be created in specified location (optional sprite atlas content txt will be created in same location and name but with txt extension)

![ssp_screen_02](https://user-images.githubusercontent.com/79088221/139771350-369e0366-2c69-48ce-9c0e-18a6e8cd6dc5.PNG)



**Importing to Unity Editor**
1. Move sprite atlas file to Unity Project (somewhere within Assets folder)

![ssp_screen_03](https://user-images.githubusercontent.com/79088221/139772115-56d31f6c-b400-425b-a91b-9607a59851e6.PNG)

3. In inspector when atlas is selected, change Texture Type to Sprite, and Sprite Mode to Multiple, then Apply changes by clicking button at the bottom right corner of inspector
4. Click Sprite Editor button

![ssp_screen_05](https://user-images.githubusercontent.com/79088221/139772253-3ad09d41-22cb-47d5-8cf8-4f6c49ccc13d.PNG)

6. Slice the sprites accordingly

![ssp_screen_06](https://user-images.githubusercontent.com/79088221/139772291-e369a055-e424-46a3-82fc-0a45a21744a0.png)

**If you want to rename sliced sprites with sprite filenames that was packed into atlas you need to have UnitySprieAtlasTool\SpriteAtlasTools.cs in your project
Generated txt file should be in the same folder as sprite atlas and have same name but with txt extension**

7. From ContextMenu when atlas is selected pick SpriteAtlas->Rename with txt file

![ssp_screen_07](https://user-images.githubusercontent.com/79088221/139772547-24cb3a44-5585-4041-b19a-af133e88bac3.png)

8. Sprite slices will be renamed 

![ssp_screen_08](https://user-images.githubusercontent.com/79088221/139772615-66ff9f6f-0285-49b5-8121-7e61efd1faf9.PNG)


Done!
