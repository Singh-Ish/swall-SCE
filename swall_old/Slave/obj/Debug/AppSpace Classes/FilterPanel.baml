   M S B A M L   `   `   `  ����E  AchildSlave, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null'clr-namespace:Prototype1
Prototype1  5          Prototype1.FilterPanel   5      R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35^ ZWindowsFormsIntegration, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     5      R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5      Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5      @d2http://schemas.microsoft.com/expression/blend/2008     5      *localclr-namespace:Prototype1     5      $��Hidden=�5   5   ��6   Z�   � 	Resources  5	         ComboBoxToggleButton �� 
TargetType � ColumnDefinitions ��Name �� CornerRadius F�Name �� Triggers P� Property P� Value	 �� 
TargetName
 �� Property �� Value  ComboBoxTextBox �� 
TargetType �� 	ClickMode d� 	IsChecked �� Path �� Mode �� RelativeSource J� IsHitTestVisible �� SelectionBoxItem �� SelectionBoxItemTemplate ��Name �� 
IsReadOnly �� 
IsReadOnly �� IsDropDownOpen � AllowsTransparency J� SnapsToDevicePixels � ActualWidth �� MaxDropDownHeight K� IsItemsHost-  'System.Windows.Input.KeyboardNavigation   DirectionalNavigation P� 
SourceName  �� Triggers! �� Padding��%T  &	        &	     '�� w    7   7  '�� �    �� 5   
   " d�67   .R�5      �  5      �� 5      �� 5      $��20 �6'   .+�5      �� $ Border��5      $��2��5      $ 5��5      ���	#FF3F3F3F���	#FF97A0A5$��0q�5      �� 5      $��0��5      $ 0��5      $��1q�5      ���	#FF3F3F3F���	#FF97A0A5$��0,0,1,0q�5      F� 5      $ Arrow��5       $��1��5!      
�������$��Center=�5#      $��Center=�5$      =����        �    �� � 	= ��[ � z �� � z     � 	=  	= 	5%      5'       5(      P� 5)       �@J�IsMouseOver . . �5*      �� $	 Border��6   
 � �� �#8080806G   5+      P� 5,       �@d�	IsChecked$
 trueY�6<   . �5-      �� $	 Border��6   
 � �� �#E0E0E06G   5.      P� 5/       � {� .  . �50      �� $	 Border��6   
 � �� �#EEEEEE6G   �� 51      $	 Border��6   
 � �� �#AAAAAA6H   �� 52      
 � �� �#88888863   �� 53      $	 Arrow��6   
 � �� �#8888886@   54      56      �� 58   
   " ��62   .R�59      �� $ PART_ContentHost��6   |�.  #��z��6A   5:      �� 5<   
   " ��6*   .�5=      �� 
 �@J�SnapsToDevicePixels . 64   �� 5>      
 �@�OverridesDefaultStyle . 66   �� 5?      
 � ��$
 Auto=�6K   �� 5@      
 � ��$
 Auto=�6I   �� 5A      
 � �� . 6>   �� 5B      
 � ��$	 120��6)   �� 5C      
 � ��$ 20��6*   �� 5D      
 � ��
 �����6+   �� 5E      
 � �� 5F      �� 5G      " ��6&   .R�5H      � .+�5I      d� $��ToggleButton��5J      8��  $��2��5L      |�.  $ Press=�5O       5N      �� $ IsDropDownOpen�$ TwoWay=� �� *TemplatedParent+5P      �� 5Q      $��ContentSite��6/    .  #��z 6\   #��z 5R      #��z��5S      $��3,3,23,3q�5T      $��Center=�5U      $
��Left=�5V      �� 5W      $ PART_EditableTextBox��6&   8�� $
��Left=�5Z      $��Center=�5[      $��3,3,23,3q�5\      |�. ���	#FF3F3F3F
��� � �$y�Hidden=�5`      # z 5a      ��5X      Z� 5a      � 5b      $��Popup��5c      $��Bottom=�5d      #��z 5e       . |�.  $��Slide=�5h      .��5j   "   � $��DropDown��6'    . #��z 5l      #��z 5m      .+�5n   &   �� $ DropDownBorder��5o   !   ���	#FF3F3F3F$��1q�5r   !   ���#8888885s   !   �� 5t   &   $��4,6,4,6q�63    . .��5u   *   ��  . $ 	Contained=�6H   5v   '   5w   #   5x      5y       5z      P� 5{       �@��HasItems .  . �5|   "   �� $	 DropDownBorder��6)   
 � ��$ 95��6Z   5}      P� 5~       � {� .  . �5   "   �� 
 � �� �#8888886?   5�      P� 5�       �@��
IsGrouping . . �5�   "   �� 
 � �� .  6R   5�      P� 5�      $ Popup��6&    �@�AllowsTransparency . . �5�   "   �� $	 DropDownBorder��6)   
 �@��CornerRadius$ 0��6]   �� 5�   "   $	 DropDownBorder��6)   
 � ��$ 0,2,0,0q�6W   5�      P� 5�       �@��
IsEditable . . �5�   "   �� 
 � �� .  6>   �� 5�   "   $	 PART_EditableTextBox��6)   
 � y�$ Visible=�6a   �� 5�   "   $	 ContentSite��6)   
 � y�$ Hidden=�6X   5�      5�      5�        5�      5�      �� 5�   
   " ��6.   .�5�      �� 
 �@J�SnapsToDevicePixels . 64   �� 5�      
 � ��
 �����6+   �� 5�      
 �@�OverridesDefaultStyle . 66   �� 5�      
 � �� 5�      �� 5�      " ��6&   .R�5�      �� $��Border��6!   $! 2q�5�       . ._�5�      �� 5�       5�      P� 5�       �@��IsHighlighted . . �5�   "   �� $	 Border��6)   
 � �� �	#FF4F4F4F6S   5�      P� 5�       � {� .  . �5�   "   �� 
 � �� �#8888886?   5�      5�      5�      5�      5�      .��5�      � -   5        $
��grid��5�      $	��Top=�6   $
��Left=�6/   ���	#FF3F3F3F#��� ~�6a   5�      5�      