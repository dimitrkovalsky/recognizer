   M S B A M L   `   `   `  ����  
recognizer    recognizer.MainWindow   5   	   W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35^ ZWindowsFormsIntegration, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   -   5          � Title$  
Recognizer��5   	   $	��390��6   $	��778��6)    � Icon$6 0icons/w128h1281371935545voicesearch128x12832.ico��6L    � ToolTip$  ��6�   .��5      �  � ColumnDefinitions 5   
   �� 5      $
��211* �6   �� 5      $
��216* �6   �� 5	      $	��90* �6   .+�5   
   �� -   5         ��Name$ historyListBox��5      $��0,0,0,0q�6*   $	��Top=�6;   $	��500��6S   $��Right=�6b   $ History��6~   $	��200��6�   $	��300��6�   $	��370��6�   $
��auto��6�   $��2��6�   $��1��6�   �� 5   
   $
��Left=�6   $��	19,28,0,0q�6+   $	��Top=�6>   �� 5   
   -   5         ��Name$ connectionLbl��5      $��not established��6'   $
��Left=�6A   $��153,123,0,0q�6\   $	��Top=�6q   �� 5   
   -   5        $ lastCommandLbl��5      $��-��6(   $
��Left=�64   $��153,153,0,0q�6O   $	��Top=�6d    J� RenderTransformOrigin$ 2.889,0.615&�6|   �� 5   
   $��Last command:��6   $
��Left=�6(   $��
24,153,0,0q�6C   $	��Top=�6W   $ 
2.64,0.231&�6o   �� 5   
   -   5        $ recStatusLbl��5      $��not started��6&   $
��Left=�6<   $��153,194,0,0q�6W   $	��Top=�6l   $ 2.311,0.577&�6�   �� 5   
   $��Recognizer status��6   $
��Left=�6,   $��
24,194,0,0q�6G   $	��Top=�6[   $ 
2.64,0.231&�6s   �� 5   
   -   5        $ serverStatusLbl��5      $��not started��6)   $
��Left=�6?   $��153,239,0,0q�6Z   $	��Top=�6o   $ 2.311,0.577&�6�   �� 5   
   $��Server status��6   $
��Left=�6(   $��
24,240,0,0q�6C   $	��Top=�6W   $ 
2.64,0.231&�6o   �� 5   
   $��Connection:��6   $
��Left=�6&   $	��Top=�6A   $��
26,123,0,0q�6Y   $ 0.342,0.538&�6m   �� 5   
   -   5         ��Name$ devicesCombo��5      $
��Left=�6)   $��	24,38,0,0q�6D   $	��Top=�6W   $	��215��6o   6{   �� 5   
   $��Device :��6   $
��Left=�6#   $	��Top=�6>   $��	24,10,0,0q�6V   $ 0.342,0.538&�6i   �� 5   
   -   5        $ activeGrammar_lbl��5      $��	Dictation��6+   $
��Left=�6?   $��153,273,0,0q�6Z   $	��Top=�6o   $ 2.311,0.577&�6�   �� 5   
   $��Active Grammar��6   $
��Left=�6)   $��
24,274,0,0q�6D   $	��Top=�6X   $ 
2.64,0.231&�6p   5      5      