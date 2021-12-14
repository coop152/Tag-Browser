using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace Tag_Browser
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public ObservableCollection<Post> AllLoadedPosts = new ObservableCollection<Post>();
        public ObservableCollection<Post> SearchResultPosts = new ObservableCollection<Post>();
        public ObservableCollection<string> SelectedPostTags = new ObservableCollection<string>();
        public FolderBrowserDialog dialog = new FolderBrowserDialog();
        public MainWindow()
        {
            InitializeComponent();
        }

        private async void LoadButton_Click(object sender, RoutedEventArgs e)
        {
            dialog.SelectedPath = Environment.CurrentDirectory;
            DialogResult result = dialog.ShowDialog();

            if (result == System.Windows.Forms.DialogResult.OK && !string.IsNullOrWhiteSpace(dialog.SelectedPath))
            {
                IEnumerable<string> files = await Task.Run(() => EnumerateFiles(dialog.SelectedPath));
                await PopulateResultsList(files);
            }
        }

        private IEnumerable<string> EnumerateFiles(string path)
        {
            string[] allPaths = Directory.GetFiles(path, "*.*", SearchOption.AllDirectories);
            return allPaths.Where(f => IsImage(f));
        }
        private bool IsImage(string file)
        {
            return file.EndsWith(".png") ||
                file.EndsWith(".jpg") ||
                file.EndsWith(".jpeg") ||
                file.EndsWith(".gif");
        }
        public void SortResultsListByArtistAsc()
        {
            try
            {
                CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(ResultsList.ItemsSource);
                SortDescription artistSort = new SortDescription("Artist", ListSortDirection.Ascending);
                view.SortDescriptions.Add(artistSort);
                SortDescription numberSort = new SortDescription("PostNumber", ListSortDirection.Ascending);
                view.SortDescriptions.Add(numberSort);
            }
            catch { }
        }
        private async Task PopulateResultsList(IEnumerable<string> files)
        {
            progressBar.Visibility = Visibility.Visible;
            //AllLoadedPosts = await MakePostsAsync(files);
            AllLoadedPosts = await Task.Run(() => MakePostsParallel(files)); //feels faster but needs sorting
            ResultsList.ItemsSource = AllLoadedPosts;
            GC.Collect();
            progressBar.Visibility = Visibility.Hidden;
        }

        private ObservableCollection<Post> MakePostsParallel(IEnumerable<string> paths)
        {
            ParallelQuery<Post> posts = paths.AsParallel().Select(path => new Post(path));
            ObservableCollection<Post> observable = new ObservableCollection<Post>(posts);
            return observable;
        }
        private void ResultsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Post SelectedObject = (Post)ResultsList.SelectedItem;
            if (SelectedObject == null)
            {
                ImageBox.Source = new BitmapImage();
            }
            else
            {
                Uri FileLocation = new Uri(SelectedObject.Path);
                ImageBox.Source = new BitmapImage(FileLocation);
                ArtistLabel.Content = "Artist: " + SelectedObject.Artist;
                SelectedPostTags = new ObservableCollection<string>(SelectedObject.Tags);
                TagsListBox.ItemsSource = SelectedPostTags; //todo make tags and artist label properly clear when image is not shown (e.g. when search takes image out of results list)
            }
        }

        private void OpenButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string path = ImageBox.Source.ToString();
                System.Diagnostics.Process.Start(path);
            }
            catch
            {

            }
        }

        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            SearchResultPosts.Clear();
            if (SearchBox.Text.Trim() == string.Empty)
            {
                ResultsList.ItemsSource = AllLoadedPosts;
            }
            else
            {
                string[] searchTags = SearchBox.Text.Trim().Split(' ');
                foreach (Post post in AllLoadedPosts)
                {
                    int tagsSatisfied = 0;
                    foreach (string tag in searchTags)
                    {
                        if (tag.StartsWith("-") && !post.Tags.Contains(tag.Substring(1)))
                        {
                            tagsSatisfied++;
                        }
                        else if (post.Tags.Contains(tag))
                        {
                            tagsSatisfied++;
                        }
                    }
                    if (tagsSatisfied == searchTags.Length)
                    {
                        SearchResultPosts.Add(post);
                    }
                }
                ResultsList.ItemsSource = SearchResultPosts;
                SortResultsListByArtistAsc();
            }
        }

        private void SearchBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                SearchButton_Click(SearchBox, new RoutedEventArgs());
            }
        }

        private void TagsListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            string selectedTag = (string)TagsListBox.SelectedItem;
            SearchBox.Text += " " + selectedTag;
            SearchButton_Click(TagsListBox, new RoutedEventArgs());
        }

        private void ResultsListRightHeader_Click(object sender, RoutedEventArgs e)
        {
            SortResultsListByArtistAsc();
        }
    }
}
