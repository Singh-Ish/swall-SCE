   M S B A M L   `   `   `  ����E  AchildSlave, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null'clr-namespace:Prototype1
Prototype1  5          Prototype1.SwipeToExit   5   	   R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35^ ZWindowsFormsIntegration, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     5      R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5      @d2http://schemas.microsoft.com/expression/blend/2008     5      Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5	      *localclr-namespace:Prototype1     5      -   5          �Name$  
mainWindow��5   *   |�.   � Title$ SwipeToExit��5       � WindowStartupLocation$ CenterOwner=�6    � Left$ 0��69    � WindowStyle$
 None=�6B    � WindowState$ 	Maximized=�6U    � AllowsTransparency . 
������  � Topmost . .��5      � -   5        $
��grid��5      #��� }�6   $��Center=�6e   $��Right=�6�   $��52��6�   |�.   K� ZIndex$ 1��6/  ��5   
   �� 5      	 �� Color$
	 Gray��6   
 �� Opacity$

 0.01Q�6+   .+�5   
   �� -   5        $��border��5      
���   �$��1q�63   $��Center=�6G   $��30��6d   $��0,0,0,0q�6p   $��Bottom=�6�   $	��300��6�    J� RenderTransformOrigin$ 0.5,0.5&�6�   $y�Hidden=�6�   |�.  $ 1��6  z�5      V� 5      .~�5       �� �� 5"      �� 5$       �� Angle$ 90Q�6&   T� 5&      5(      ��5,      �� 5.       �� EndPoint$ 0.5,1&�6&    � MappingMode$ RelativeToBoundingBox=�67    �� 
StartPoint$	 0,0&�6[    � SpreadMethod$ Reflect=�6l    �� RelativeTransform 50      V� 52      .~�54      ��  �� ScaleY$ 1Q�6-    �� ScaleX$ 1Q�68   �� 56       �� AngleY$ 0Q�6,    �� AngleX$ 0Q�67   �� 58      $ 0Q�6.   T� 5:      5<      .��5@      �  � Color$
 Gray��6#   � 5B      $ White��6#    � Offset$ 1Q�61   � 5D      $ 	#FF010101��6#   $ 0.762Q�65   5F      5J      �� 5L   
   -   5        $��Swipe��5L      
�������$��Right=�61   $��Center=�6M   $��52��6h   
������ $��1q�6�   $��0,0,0,0q�6�   #��� }�6�   |�.  $ 1��6:  ._�5N      �� -   5         ��Name$ textBox��5N      $��Center=�6'    �� TextWrapping$ NoWrap=�6D   $��Swipe to exit��6Z   $��Center=�6o   $ 0.5,0.5&�6�   $��25(�6�   
���   �$��-420,0,-420,0q�6�   
������  �� TextAlignment$ Center=�6�   
������ {�.  $	��890��6>  |�.  .��6�  z�5P      V� 5R      .~�5T      �� �� 5V      �� 5X      $	 270Q�6*   T� 5Z      5\      .��5^   +   5`      5b      5d      5f      