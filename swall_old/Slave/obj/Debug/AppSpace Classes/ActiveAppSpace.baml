   M S B A M L   `   `   `  ����E  AchildSlave, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null'clr-namespace:Prototype1
Prototype1  5      I)clr-namespace:Prototype1.Activity_ClassesPrototype1.Activity_Classes  5      5clr-namespace:Prototype1.StylesPrototype1.Styles  5          Prototype1.ActiveAppSpace   5      R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35^ ZWindowsFormsIntegration, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     5      R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5      Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5      @d2http://schemas.microsoft.com/expression/blend/2008     5      *localclr-namespace:Prototype1     5      >activity)clr-namespace:Prototype1.Activity_Classes     5      1styleclr-namespace:Prototype1.Styles     5      -   5          >�Name$  userControl��5      #��� ~�5
      
������ $��Center=�5      $��Center=�6*    � 	Resources 5      1   +Prototype1.Styles.BoolToVisibilityConverter  5   
      KeyBoolToVisibilityConverter  6*   �� 5   
   %!descriptionTextBoxControlTemplate   �� 
TargetType" ��6D   .R�5      ��  ��Name$ PART_ContentHost��5       J� SnapsToDevicePixels# z 5      5      �� 5   
   FadeOut  .��5      W�  g� Duration$ 0:0:1D�6    �� TargetProperty$ Opacity�6/    W� From$ 1X�6S   	 W� To$ 0X�6\   5      .��5      � .+�5   
   � -   5        	 �Name$
	 grid��5      
���   �$
��Auto��60   $y�	Collapsed=�6�   $��0,0,0,0q�6�   $��Bottom=�6�   
 J� Effect
 5      3  -System.Windows.Media.Effects.DropShadowEffect  5         ShadowDepth$ 0Q�6#      Color$	 Red��63      Opacity$	 1.0Q�6?      
BlurRadius$
 10.0Q�6M   z�5      T� 5       -   5        gtt��5    %   .+�5"      ]� -   5         ]�Name$ MainMenu��5"      .+�5#         Prototype1.MenuHeader  -   5          Name$ header��5#   8   $	��Top=�6#   #��� ~�6H      Prototype1.FilterPanel  5$      -   5          Name$ FilterSection��5$   $   $	��Top=�6;   �� 5%      -   5         ��Name$ button��5%      $��Close��6)   $��30��69   $	��Top=�6E   $��75��6]   $	��Top=�6h   $��Right=�6}   6�      Prototype1.ActivityMatrix  5&      -   5          Name$ ActivitySection��5&   '   .   (Prototype1.Activity_Classes.ActivityView   ActivityActivated$' !ActivitySection_ActivityActivated��5'   #      
FilterTags 5(   #   �� *FiltersTags+ �� ElementName$ FilterSection��   	SearchTag 5)   #   �� *	SearchTag+$ header��5*      5+      � 5,   
   -	   5        $��transitionGrid��5,      
���   � K� ZIndex$ 1��68   $y�Visible=�5-      .+�5.      r� -
   5         r�Name$ 
transition��5.       r� UnloadedBehavior$ Manual=�50      51      53      54      