   M S B A M L   `   `   `  ����H  DProjectKinect, Version=2.0.0.0, Culture=neutral, PublicKeyToken=null-clr-namespace:ProjectKinectProjectKinect  5   	   "    ProjectKinect.PostureCapture   5   	   R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   d `Microsoft.Kinect.Wpf.Controls, Version=2.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e350k(http://schemas.microsoft.com/kinect/2014  5   	   'localclr-namespace:ProjectKinect  5   	   -   5          � WindowStyle$
  None=�5   	    � 
ResizeMode$ NoResize=�6    � WindowState$ 	Maximized=�62    � Title$ PostureCapture��5
   	   $
��1920��6    $
��1080��6.   
���   �.��5      0  *Microsoft.Kinect.Wpf.Controls.KinectRegion  -   5          Name$ kinectRegion��5      .��5      � .+�5   
   (� $	��top=�6   $
��1920��6*   $
��1080��68   $��	0,0,0.0,0q�6E   z�5      V� 5      .~�5      ��  �� ScaleX$ 1Q�6%    �� ScaleY$
 3.16Q�60   �� 5      �� 5       �� Angle$ 0Q�6&   T� 5      5      .��5      � $
��1920��6   $
��1080��6    .+�5      �� -   5        $��camera��5      �� 5      -   5        $��canvas��5      5      5      � 5       � RowDefinitions 5      �� 5      $	��200 �6    �� 5      $
��1720 �6    .+�5!      � $��0��6   $��0 0 0 0q�6    	 � ColumnDefinitions	 5"      �� 5#      $	��200 �6'   �� 5$      $	��880 �6'   5&      �� 5'      -   5        
 ��Name$
 Posture��5'      $��Right=�6)   $��Center=�6E   $��Images\styler.png��6`   $��0,70,120.10,70q�6{   $��0��6�   $��1��6�   $��50��6�   $	��200��6�   �� 5(      $��Right=�6   $��
0,60,40,60q�68   
�������$��36(�6_   $��Gabriola)�6m   $��0��6�   $��1��6�   Styler6�   �� 5+      -   5         ��Name$ navigationRegion��5+   .   $��1��6!   $��0,0,0,0q�6H   .��5,      �� -   5         ��Name$ 
Screenshot��5,      $��
Screenshot��61   $��Right=�6F   $��	0,670,0,0q�6b   $	��Top=�6u   $	��200��6�   $	��100��6�   6�   5-      5.      5/      50      51      