   M S B A M L   `   `   `  ����E  AchildSlave, Version=1.0.0.0, Culture=neutral, PublicKeyToken=nullI)clr-namespace:Prototype1.Activity_ClassesPrototype1.Activity_Classes  5      5clr-namespace:Prototype1.StylesPrototype1.Styles  5      -    'Prototype1.Activity_Classes.MediaPlayer   5      R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35^ ZWindowsFormsIntegration, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     5      R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5      Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5      @d2http://schemas.microsoft.com/expression/blend/2008     5      ;local)clr-namespace:Prototype1.Activity_Classes     5      1styleclr-namespace:Prototype1.Styles     5      -   5          >�Name$  userControl��5      $
��Left=�5	   9    � 	Resources 5       
  FadeIn g� Duration �� TargetProperty W� From	 W� To  FadeOut  ButtonControlTemplate1 �� 
TargetType �Name'  !System.Windows.VisualStateManager   VisualStateGroups%  System.Windows.VisualStateGroup	   Transitions%  System.Windows.VisualTransition
   From   GeneratedDuration	   To   States   System.Windows.VisualState   
Storyboard �� 
TargetName9  3System.Windows.Media.Animation.EasingDoubleKeyFrame O� KeyTime O� Value R� KeyTime"  System.Windows.Media.Stretch   Fill R� Value ��Name J� Opacity  ButtonControlTemplate2��%
  &	        &	 h     &	 �     &	 c    �� 5   
   .��5      W� $ 0:0:0.5D�6   $ Opacity�61   $ 0X�6U   $ 1X�6^   5      �� 5   
   -   5        .��5      W� $ 0:0:.5D�6   $ Opacity�60   $ 1X�6T   $ 0X�6]   5      �� 5   
   " ��69   .R�5      � $
 grid��6    5        5      CommonStates��	 5        5      $
 Normal��6/   $ 0:0:0.01D�6=   $ Pressed��6Z     5      $
 	MouseOver��6/   $ 0:0:0.1D�6@   $ Normal��6\     5      $
 Normal��6/   $ 0:0:0.1D�6=   $ 	MouseOver��6Y     5      $
 	MouseOver��6/   $ 0:0:0.01D�6@   $ Pressed��6]     5      $ 0:0:0.1D�6/     5      $
 Pressed��6/   $ 0:0:0.2D�6>   . 5         
Normal��6&     5!      	MouseOver��. 5"      �� .��5#   "   U� $ (UIElement.Opacity)�6@   $ image��6p   .\�5$   &     $ 0��6;   $ 1Q�6G   5%   #   5&      5'        5(      Pressed��. 5)      �� .��5*   "   T� $ (Viewbox.Stretch)�6@   $ image��6n   ..�5+   &   i� $ 0��6=   # Z 6I   5,   #   U� 5-   "   $ (UIElement.Opacity)�6@   $ image��6p   .\�5.   &     $ 0��6;   $ 1Q�6G   5/   #   50      51        52      Disabled��6&   53      .+�55      �� $ image��6   $	 0.5Q�6'   $6��0/Prototype1;component/Resources/Buttons/left.png��65   56      57      �� 58   
   " ��69   .R�59      � $
 grid��6    5:        5;      CommonStates��	 5<        5=      $
 Normal��6/   $ 0:0:0.01D�6=   $ Pressed��6Z     5>      $
 	MouseOver��6/   $ 0:0:0.1D�6@   $ Normal��6\     5?      $
 Normal��6/   $ 0:0:0.1D�6=   $ 	MouseOver��6Y     5@      $
 	MouseOver��6/   $ 0:0:0.01D�6@   $ Pressed��6]     5A      $ 0:0:0.1D�6/     5B      $
 Pressed��6/   $ 0:0:0.2D�6>   . 5D        
Normal��6&     5E      	MouseOver��. 5F      �� .��5G   "   U� $ (UIElement.Opacity)�6@   $ image��6p   .\�5H   &     $ 0��6;   $ 1Q�6G   5I   #   5J      5K        5L      Pressed��. 5M      �� .��5N   "   T� $ (Viewbox.Stretch)�6@   $ image��6n   ..�5O   &   i� $ 0��6=   # Z 6I   5P   #   U� 5Q   "   $ (UIElement.Opacity)�6@   $ image��6p   .\�5R   &     $ 0��6;   $ 1Q�6G   5S   #   5T      5U        5V      Disabled��6&   5W      .+�5Y      �� $ image��6   $	 0.5Q�6'   $7��1/Prototype1;component/Resources/Buttons/right.png��65   5Z      5[      5\      .��5^      � -   5        $
 grid��5^      
���   �.+�5_   
   2   ,Prototype1.Activity_Classes.VideoAudioPlayer  -   5          Name$ mediaSlideshowVideo��5_   !   $��10q�5a   !   $   InteractiveElementsVisibility 5`   !   �� *IsInteractive+ �� ElementName$ userControl��5a   !   �� 5c   
   -   5        $ mediaSlideshowImage��5c      $
��Fill=�6-   $��10q�6<   �� 5e   
   -   5         ��Name$ button��5e      $��X��6!   $��Right=�6-   $	��Top=�6I   $��1q�5g   &   $
��Bold"�62   $��Arial)�6D   $��70(�6W   ��5e   a   Z� y�5f      �� *InteractiveElementsVisibility+$ userControl�� �� Mode$ OneWay=���5h      
� 5i      .��5j      �  � Color$ White��6#    � Offset$ 0Q�61   � 5k      $	 Red��6#   $ 0.568Q�6/   � 5l      $ Transparent��6#   $ 1Q�67   5m      5o      �� 5p   
   -   5        $ 
backButton��5p      $��0,200,1500,200q�5q      #���  5r      y�5p   %   �� *IsSlideshow+$ userControl��$ OneWay=���5q   )   Z� ��5t      �� 5u       �� ImageSource$6 0/Prototype1;component/Resources/Buttons/left.png��6    �� Opacity$	 0.3Q�6\   5w      �� 5x   
   -   5        $ forwardButton��5x      $��1500,200,0,200q�5y      #���  6\   y�5x   (   �� *IsSlideshow+$ userControl��$ OneWay=���5y   )   Z� ��5z      �� 5{      $7 1/Prototype1;component/Resources/Buttons/right.png��6   $	 0.3Q�6]   5}      5~      5�      