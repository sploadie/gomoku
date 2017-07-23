/* ************************************************************************** */
/*                                                                            */
/*                                                        :::      ::::::::   */
/*   gomoku.h                                           :+:      :+:    :+:   */
/*                                                    +:+ +:+         +:+     */
/*   By: tgauvrit <tgauvrit@student.42.fr>          +#+  +:+       +#+        */
/*                                                +#+#+#+#+#+   +#+           */
/*   Created: 2016/12/17 11:47:16 by tgauvrit          #+#    #+#             */
/*   Updated: 2017/01/31 12:48:34 by tgauvrit         ###   ########.fr       */
/*                                                                            */
/* ************************************************************************** */

#ifndef GOMOKU_H
# define GOMOKU_H

# include <stdlib.h>
# include <string.h>
# include <unistd.h>
# include <stdio.h>
# include <fcntl.h>
# include <limits.h>
# include <time.h>
# include <stdlib.h>

# define BOARD_FILE_SIZE 245
# define MAX_MOVES 192
# define EMPTY '.'
# define TEMPT '@'
# define WHITE 'w'
# define BLACK 'b'
# define ERROR 'E'

# ifdef USE_INT
typedef uint32_t		t_space;
# else
typedef unsigned char	t_space;
# endif

typedef struct			s_heuristic
{
	int		free3;    // 12% (4%  per free3, max 3)
	int		free4;    // 20% (10% per free4, max 2)
	int		line5;    // 30% (MAX/MIN if opposing player turn)
	int		captured; // 36% (4%  per capture, max 9)
}						t_heuristic;

typedef struct			s_move
{
	uint32_t			x;
	uint32_t			y;
	int					h; // for sorting only
}						t_move;

typedef struct			s_ab
{
	int					alpha;			// maximizing
	int					beta;			// minimizing
}						t_ab;

typedef struct			s_player
{
	t_space				color;
	int					maximizing;
}						t_player;

typedef struct			s_board
{
	t_space				map[15][15];
	t_heuristic			hrc[2];
	int					h;
}						t_board;

// GLOBALS
// Must be >= 1:
extern int	g_alphabeta_depth;

// FUNCTION DECLARATIONS
t_board		*read_board(int fd, t_player *player);
t_board		*parse_board(char *string, t_player *player);
void		print_board(t_board *board, t_player player);

t_player	player_create(int color, int maximizing);
t_player	player_switch(t_player player);
t_player	player_maximizing(t_player player);

t_ab		ab_new(void);
t_move		alphabeta(t_board *board, int color);

t_move		move_create(t_space x, t_space y);
int			moves_get(t_board board, t_player player, t_move *moves);
int			moves_get_boards(t_board *board_model, t_player player, t_board *move_boards);
void		move_boards_sort(t_board *move_boards, int size);

t_board		*heuristic_partial_move(t_board *board, t_player one, int move_x, int move_y);
int			heuristic_calculate(t_board *board, int is_maximizing);

int			board_isvalid(int x, int y);
void		board_line5(t_board *board, t_player p);
void		board_free(t_board *board, t_player p);
void		point_all(t_board *board, t_player player, int x, int y, int mod);
void		board_capture(t_board *board, t_player p, int x, int y);

void		*memdup(void *data, size_t len);

#endif
